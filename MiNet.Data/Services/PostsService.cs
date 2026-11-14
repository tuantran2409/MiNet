using Microsoft.EntityFrameworkCore;
using MiNet.Data.Dtos;
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class PostsService : IPostsService
    {
        private readonly AppDbContext _context;
        private readonly INotificationsService _notificationService;
        private readonly IRecommendationService _recommendationService;
        private readonly IUserInteractionService _interactionService;

        public PostsService(
            AppDbContext context,
            INotificationsService notificationService,
            IRecommendationService recommendationService,
            IUserInteractionService interactionService)
        {
            _context = context;
            _notificationService = notificationService;
            _recommendationService = recommendationService;
            _interactionService = interactionService;
        }

        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            return await GetPersonalizedFeedAsync(loggedInUserId);
        }

        public async Task<List<Post>> GetPersonalizedFeedAsync(int currentUserId, int page = 1, int pageSize = 30)
        {
            // 1. Get friend IDs
            var friendIds = await _context.Friendships
                .AsNoTracking()
                .Where(n => n.SenderId == currentUserId || n.ReceiverId == currentUserId)
                .Select(n => n.SenderId == currentUserId ? n.ReceiverId : n.SenderId)
                .ToListAsync();

            // 2. Fetch candidate pool
            var candidatePosts = await _context.Posts
                .AsNoTracking()
                .Where(n => (!n.IsPrivate || n.UserId == currentUserId || friendIds.Contains(n.UserId)) && n.Reports.Count < 5 && !n.IsDeleted)
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(c => c.User)
                .Include(n => n.Reports)
                .OrderByDescending(n => n.DateCreated)
                .Take(150)
                .ToListAsync();

            if (!candidatePosts.Any())
                return new List<Post>();

            // 3. User interaction count & interests for Cold-Start check
            int interactionCount = await _interactionService.GetUserInteractionsCountAsync(currentUserId);
            var userInterests = await _interactionService.GetUserInterestsAsync(currentUserId);
            bool isColdStart = interactionCount < 5;

            // 4. Calculate FinalScore for each candidate post
            var scoredPosts = candidatePosts.Select(post =>
            {
                // A. ML.NET Predicted AI Score [0, 1]
                float aiScore = _recommendationService.PredictScore(currentUserId, post.Id);

                // B. Relationship Weight [0, 1]
                float relationshipWeight = 0.2f;
                if (post.UserId == currentUserId || friendIds.Contains(post.UserId))
                {
                    relationshipWeight = 1.0f;
                }

                // C. Time Decay [0, 1]
                double elapsedHours = Math.Max(0.05, (DateTime.UtcNow - post.DateCreated).TotalHours);
                float timeDecay = 1.0f / (1.0f + 0.05f * (float)elapsedHours);

                // Formula: FinalScore = (0.60 * AIScore) + (0.25 * RelationshipWeight) + (0.15 * TimeDecay)
                float finalScore = (0.60f * aiScore) + (0.25f * relationshipWeight) + (0.15f * timeDecay);

                // D. Cold-Start handling
                if (isColdStart)
                {
                    if (userInterests.Any() && !string.IsNullOrWhiteSpace(post.Content))
                    {
                        string contentLower = post.Content.ToLower();
                        if (userInterests.Any(tag => contentLower.Contains(tag.ToLower())))
                        {
                            finalScore += 0.40f;
                        }
                    }

                    if (elapsedHours <= 48)
                    {
                        int engagement = post.Likes.Count + post.Comments.Count + post.Favorites.Count;
                        finalScore += Math.Min(engagement * 0.05f, 0.30f);
                    }
                }

                return new { Post = post, Score = finalScore };
            })
            .OrderByDescending(x => x.Score)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Post)
            .ToList();

            return scoredPosts;
        }

        public async Task<List<Post>> GetExplorePostsAsync(int count = 30)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => !p.IsDeleted && !p.IsPrivate && p.NrOfReports < 5 && !string.IsNullOrEmpty(p.ImageUrl))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.Likes.Count * 2 + p.Comments.Count * 3)
                .ThenByDescending(p => p.DateCreated)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            return await _context.Posts
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == postId);
        }

        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Include(f => f.Post.Reports)
                .Include(f => f.Post.User)
                .Include(f => f.Post.Comments).ThenInclude(c => c.User)
                .Include(f => f.Post.Likes)
                .Include(f => f.Post.Favorites)
                .Where(n => n.UserId == loggedInUserId && !n.Post.IsDeleted && n.Post.Reports.Count < 5)
                .OrderByDescending(f => f.DateCreated)
                .Select(n => n.Post)
                .ToListAsync();
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            // Self-interaction as creator
            await _interactionService.RecordInteractionAsync(post.UserId, post.Id, InteractionType.Save, 0);

            return post;
        }

        public async Task<Post?> RemovePostAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);
            if (postDb != null)
            {
                postDb.IsDeleted = true;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();
            }
            return postDb;
        }

        public async Task ReportPostAsync(int postId, int userId)
        {
            var newReport = new Report
            {
                PostId = postId,
                UserId = userId,
                DateCreated = DateTime.UtcNow
            };

            await _context.Reports.AddAsync(newReport);
            await _context.SaveChangesAsync();

            var post = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);
            if (post != null)
            {
                post.NrOfReports += 1;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemovePostCommentAsync(int commentId)
        {
            var commentDb = await _context.Comments.FirstOrDefaultAsync(n => n.Id == commentId);
            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPostCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Record AI interaction: Comment (+5.0 score)
            await _interactionService.RecordInteractionAsync(comment.UserId, comment.PostId, InteractionType.Comment);
        }

        public async Task<GetNotificationDto> TogglePostLikeAsync(int postId, int userId)
        {
            var response = new GetNotificationDto
            {
                Success = true,
                SendNotification = false
            };

            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.AddAsync(newLike);
                await _context.SaveChangesAsync();

                // Record AI interaction: Like (+3.0 score)
                await _interactionService.RecordInteractionAsync(userId, postId, InteractionType.Like);

                response.SendNotification = true;
            }

            return response;
        }

        public async Task<GetNotificationDto> TogglePostFavoriteAsync(int postId, int userId)
        {
            var response = new GetNotificationDto
            {
                Success = true,
                SendNotification = false
            };

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.PostId == postId && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite
                {
                    PostId = postId,
                    UserId = userId,
                    DateCreated = DateTime.UtcNow
                };
                await _context.AddAsync(newFavorite);
                await _context.SaveChangesAsync();

                // Record AI interaction: Save (+7.0 score)
                await _interactionService.RecordInteractionAsync(userId, postId, InteractionType.Save);

                response.SendNotification = true;
            }

            return response;
        }

        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postId && l.UserId == userId);

            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
