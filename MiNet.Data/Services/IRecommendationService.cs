using System;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class ModelMetricsDto
    {
        public bool IsTrained { get; set; }
        public DateTime? LastTrainedAt { get; set; }
        public double Rmse { get; set; }
        public double RSquared { get; set; }
        public int TotalSamples { get; set; }
        public TimeSpan TrainingDuration { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
    }

    public interface IRecommendationService
    {
        Task<ModelMetricsDto> TrainModelAsync();
        float PredictScore(int userId, int postId);
        ModelMetricsDto GetModelStatus();
        bool ModelFileExists();
    }
}
