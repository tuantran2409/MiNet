using MiNet.Data.Models;
using MiNet.Data.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MiNet.Data.Services
{
    public class PostsService : IPostsService
    {
        private readonly AppDbContext _context;
        private readonly INotificationsService _notificationService;

        public PostsService(AppDbContext context, INotificationsService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            var allPosts = await _context.Posts
                .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && n.Reports.Count < 5 && !n.IsDeleted)
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(n => n.User)
                .Include(n => n.Reports)
                .Include(n => n.RepoShare)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return allPosts;
        }   
        public async Task<Post> GetPostByIdAsync(int postId)
        {
            var postDb = await _context.Posts
               .Include(n => n.User)
               .Include(n => n.Likes)
               .Include(n => n.Favorites)
               .Include(n => n.Comments).ThenInclude(n => n.User)
               .Include(n => n.RepoShare)
               .FirstOrDefaultAsync(n => n.Id == postId);

            return postDb;
        }

        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        {
            var allFavoritedPosts = await _context.Favorites
                .Include(f => f.Post.Reports)
                .Include(f => f.Post.User)
                .Include(f => f.Post.Comments)
                    .ThenInclude(c => c.User)
                .Include(f => f.Post.Likes)
                .Include(f => f.Post.Favorites)
                .Include(f => f.Post.RepoShare)
                    .Where(n => n.UserId == loggedInUserId &&
                    !n.Post.IsDeleted &&
                    n.Post.Reports.Count < 5)
                .OrderByDescending(f => f.DateCreated)
                .Select(n => n.Post)
                .ToListAsync();

            return allFavoritedPosts;
        }

        public async Task<Post> CreatePostAsync(Post post) 
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return post;
        }

        /// <summary>
        /// Updates the text of a post the user owns. Returns null when the post does not exist
        /// or belongs to somebody else.
        /// </summary>
        public async Task<Post?> UpdatePostContentAsync(int postId, int userId, string? content)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);
            if (post == null) return null;

            var newContent = content?.Trim() ?? string.Empty;

            // Saving without a change must not mark the post as edited
            if (post.Content == newContent)
                return await GetPostByIdAsync(postId);

            post.Content = newContent;
            post.DateUpdate = DateTime.Now;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return await GetPostByIdAsync(postId);
        }

        /// <summary>
        /// Creates a feed post carrying a GitHub repo card. The repo values are a snapshot
        /// taken at share time; the post content holds the user's optional comment.
        /// </summary>
        public async Task<Post> CreateRepoSharePostAsync(int userId, GitHubRepoDto repo, string? comment)
        {
            var now = DateTime.Now;

            var post = new Post
            {
                Content = comment?.Trim() ?? string.Empty,
                DateCreated = now,
                DateUpdate = now,
                NrOfReports = 0,
                UserId = userId
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            var share = new PostRepoShare
            {
                PostId = post.Id,
                RepoFullName = repo.FullName,
                RepoHtmlUrl = repo.HtmlUrl,
                OwnerAvatarUrl = repo.OwnerAvatarUrl,
                Description = Truncate(repo.Description, 1000),
                Language = repo.Language,
                Stars = repo.Stars,
                Forks = repo.Forks,
                DateCreated = now
            };

            await _context.PostRepoShares.AddAsync(share);
            await _context.SaveChangesAsync();

            post.RepoShare = share;
            return post;
        }

        /// <summary>
        /// True when the user already shared this repo after <paramref name="since"/>.
        /// Used to stop the same user spamming the same repo.
        /// </summary>
        public async Task<bool> HasUserSharedRepoSinceAsync(int userId, string repoFullName, DateTime since)
        {
            return await _context.PostRepoShares
                .AnyAsync(r => r.RepoFullName == repoFullName
                               && r.DateCreated >= since
                               && r.Post.UserId == userId
                               && !r.Post.IsDeleted);
        }

        public async Task<Post> RemovePostAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);

            if (postDb != null)
            {
                //_context.Posts.Remove(postDb);
                postDb.IsDeleted = true;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();
            }

            return postDb;
        }

        public async Task ReportPostAsync(int postId, int userId)
        {
            var newReport = new Report()
            {
                PostId = postId,
                UserId = userId,
                DateCreated = DateTime.Now
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

            if(commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPostCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<GetNotificationDto> TogglePostLikeAsync(int postId, int userId)
        {
            var response = new GetNotificationDto()
            {
                Success = true,
                SendNotification = false
            };

            var like = await _context.Likes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if(like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.AddAsync(newLike);
                await _context.SaveChangesAsync();

                response.SendNotification = true;
            }

            return response;
        }

        public async Task<GetNotificationDto> TogglePostFavoriteAsync(int postId, int userId)
        {
            var response = new GetNotificationDto()
            {
                Success = true,
                SendNotification = false
            };

            var favorite = await _context.Favorites
                .Where(f => f.PostId == postId && f.UserId == userId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postId,
                    UserId = userId,
                    DateCreated = DateTime.Now,
                };
                await _context.AddAsync(newFavorite);
                await _context.SaveChangesAsync();

                response.SendNotification = true;
            }
            return response;
        }

        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {
            //get post by id and loggedIn user id
            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postId && l.UserId == userId);

            if(post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
