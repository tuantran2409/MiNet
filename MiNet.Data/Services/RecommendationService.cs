using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using MiNet.Data.MLModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _context;
        private static readonly MLContext _mlContext = new MLContext(seed: 42);
        private static ITransformer? _trainedModel;
        private static PredictionEngine<PostInteractionData, PostScorePrediction>? _predictionEngine;
        private static readonly object _syncLock = new object();
        private static ModelMetricsDto _cachedMetrics = new ModelMetricsDto
        {
            IsTrained = false,
            StatusMessage = "Model has not been trained yet."
        };
        private readonly string _modelFilePath;

        public RecommendationService(AppDbContext context)
        {
            _context = context;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var modelDir = Path.Combine(baseDir, "MLModels");
            if (!Directory.Exists(modelDir))
            {
                Directory.CreateDirectory(modelDir);
            }
            _modelFilePath = Path.Combine(modelDir, "MinetRecModel.zip");
            _cachedMetrics.ModelPath = _modelFilePath;

            EnsureModelLoaded();
        }

        public bool ModelFileExists() => File.Exists(_modelFilePath);

        public ModelMetricsDto GetModelStatus()
        {
            _cachedMetrics.IsTrained = _trainedModel != null && ModelFileExists();
            return _cachedMetrics;
        }

        private void EnsureModelLoaded()
        {
            if (_trainedModel != null && _predictionEngine != null) return;

            lock (_syncLock)
            {
                if (_trainedModel != null && _predictionEngine != null) return;

                if (File.Exists(_modelFilePath))
                {
                    try
                    {
                        DataViewSchema modelSchema;
                        _trainedModel = _mlContext.Model.Load(_modelFilePath, out modelSchema);
                        _predictionEngine = _mlContext.Model.CreatePredictionEngine<PostInteractionData, PostScorePrediction>(_trainedModel);
                        _cachedMetrics.IsTrained = true;
                        _cachedMetrics.StatusMessage = "Model loaded successfully from disk.";
                    }
                    catch (Exception ex)
                    {
                        _cachedMetrics.StatusMessage = $"Failed to load existing model: {ex.Message}";
                    }
                }
            }
        }

        public async Task<ModelMetricsDto> TrainModelAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Gather interaction data
            var interactions = await _context.UserInteractions
                .AsNoTracking()
                .Select(ui => new PostInteractionData
                {
                    UserId = (float)ui.UserId,
                    PostId = (float)ui.PostId,
                    Label = ui.WeightScore
                })
                .ToListAsync();

            // Fallback enrichment from Likes, Favorites, Comments if interaction table is sparse
            if (interactions.Count < 20)
            {
                var likes = await _context.Likes
                    .AsNoTracking()
                    .Select(l => new PostInteractionData { UserId = (float)l.UserId, PostId = (float)l.PostId, Label = 3.0f })
                    .ToListAsync();
                interactions.AddRange(likes);

                var favorites = await _context.Favorites
                    .AsNoTracking()
                    .Select(f => new PostInteractionData { UserId = (float)f.UserId, PostId = (float)f.PostId, Label = 7.0f })
                    .ToListAsync();
                interactions.AddRange(favorites);

                var comments = await _context.Comments
                    .AsNoTracking()
                    .Select(c => new PostInteractionData { UserId = (float)c.UserId, PostId = (float)c.PostId, Label = 5.0f })
                    .ToListAsync();
                interactions.AddRange(comments);
            }

            // Remove potential duplicates by grouping by (UserId, PostId) and taking max score
            var dataset = interactions
                .GroupBy(i => new { i.UserId, i.PostId })
                .Select(g => new PostInteractionData
                {
                    UserId = g.Key.UserId,
                    PostId = g.Key.PostId,
                    Label = g.Max(x => x.Label)
                })
                .ToList();

            if (dataset.Count < 5)
            {
                stopwatch.Stop();
                return new ModelMetricsDto
                {
                    IsTrained = false,
                    TotalSamples = dataset.Count,
                    TrainingDuration = stopwatch.Elapsed,
                    StatusMessage = $"Insufficient training data (found {dataset.Count} samples, minimum 5 required). Please seed or perform user interactions."
                };
            }

            // 2. Load into IDataView
            IDataView dataView = _mlContext.Data.LoadFromEnumerable(dataset);

            // 3. Train-Test Split (80% / 20%)
            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 42);

            // 4. Build Pipeline
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserIdEncoded",
                MatrixRowIndexColumnName = "PostIdEncoded",
                LabelColumnName = nameof(PostInteractionData.Label),
                NumberOfIterations = 30,
                ApproximationRank = 64,
                LearningRate = 0.05f
            };

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "UserIdEncoded",
                    inputColumnName: nameof(PostInteractionData.UserId))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "PostIdEncoded",
                    inputColumnName: nameof(PostInteractionData.PostId)))
                .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

            // 5. Train Model
            var model = pipeline.Fit(split.TrainSet);

            // 6. Evaluate
            var predictions = model.Transform(split.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(PostInteractionData.Label), scoreColumnName: "Score");

            // 7. Save model to disk
            lock (_syncLock)
            {
                _mlContext.Model.Save(model, dataView.Schema, _modelFilePath);
                _trainedModel = model;
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<PostInteractionData, PostScorePrediction>(_trainedModel);

                _cachedMetrics = new ModelMetricsDto
                {
                    IsTrained = true,
                    LastTrainedAt = DateTime.UtcNow,
                    Rmse = double.IsNaN(metrics.RootMeanSquaredError) ? 0.0 : metrics.RootMeanSquaredError,
                    RSquared = double.IsNaN(metrics.RSquared) ? 0.0 : metrics.RSquared,
                    TotalSamples = dataset.Count,
                    TrainingDuration = stopwatch.Elapsed,
                    StatusMessage = "Model successfully trained and persisted.",
                    ModelPath = _modelFilePath
                };
            }

            stopwatch.Stop();
            return _cachedMetrics;
        }

        public float PredictScore(int userId, int postId)
        {
            EnsureModelLoaded();

            if (_predictionEngine == null)
            {
                return 0.5f; // Neutral fallback if model is not available
            }

            lock (_syncLock)
            {
                try
                {
                    var input = new PostInteractionData
                    {
                        UserId = (float)userId,
                        PostId = (float)postId
                    };

                    var prediction = _predictionEngine.Predict(input);
                    float rawScore = prediction.Score;

                    if (float.IsNaN(rawScore) || float.IsInfinity(rawScore))
                    {
                        return 0.5f;
                    }

                    // Sigmoid normalization to [0, 1]
                    float sigmoid = 1.0f / (1.0f + (float)Math.Exp(-rawScore));
                    return sigmoid;
                }
                catch
                {
                    return 0.5f;
                }
            }
        }
    }
}
