using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNet.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MiNet.Data.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== Dashboard ====================
        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var monthAgo = now.AddMonths(-1);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPosts = await _context.Posts.Where(p => !p.IsDeleted).CountAsync(),
                TotalReports = await _context.Reports.CountAsync(),
                OnlineUsers = 0, // Sẽ cập nhật nếu có tracking online status
                ReportedPosts = await GetReportedPostsAsync(),
                TopPosts = await GetTopPostsAsync(5),
                NewUsersThisMonth = await _context.Users
                    .Where(u => u.LockoutEnd == null || u.LockoutEnd < now)
                    .CountAsync(),
                NewPostsThisMonth = await _context.Posts
                    .Where(p => !p.IsDeleted && p.DateCreated >= monthAgo)
                    .CountAsync()
            };

            return model;
        }

        // ==================== Reported Posts ====================
        public async Task<List<Post>> GetReportedPostsAsync()
        {
            var posts = await _context.Posts
                .Include(n => n.User)
                .Include(n => n.Reports)
                .Where(n => n.NrOfReports > 0 && !n.IsDeleted)
                .OrderByDescending(p => p.NrOfReports)
                .ToListAsync();

            return posts;
        }

        public async Task<List<Report>> GetReportsForPostAsync(int postId)
        {
            return await _context.Reports
                .Include(r => r.User)
                .Where(r => r.PostId == postId)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task ApproveReportAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);

            if (postDb != null)
            {
                postDb.IsDeleted = true;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectReportAsync(int postId)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(n => n.Id == postId);

            if (postDb != null)
            {
                postDb.NrOfReports = 0;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();
            }

            var postReports = await _context.Reports.Where(n => n.PostId == postId).ToListAsync();
            if (postReports.Any())
            {
                _context.Reports.RemoveRange(postReports);
                await _context.SaveChangesAsync();
            }
        }

        // ==================== User Management ====================
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.Id)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Posts)
                .Include(u => u.Comments) 
                .Include(u => u.Likes)  
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task LockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100); // Khóa vĩnh viễn
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnlockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // 1. Delete notifications belonging to this user
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            _context.Notifications.RemoveRange(notifications);

            // 2. Get all post IDs owned by this user
            var userPostIds = await _context.Posts
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            if (userPostIds.Any())
            {
                // 3. Delete likes on user's posts
                var likesOnPosts = await _context.Likes
                    .Where(l => userPostIds.Contains(l.PostId))
                    .ToListAsync();
                _context.Likes.RemoveRange(likesOnPosts);

                // 4. Delete comments on user's posts
                var commentsOnPosts = await _context.Comments
                    .Where(c => userPostIds.Contains(c.PostId))
                    .ToListAsync();
                _context.Comments.RemoveRange(commentsOnPosts);

                // 5. Delete favorites on user's posts
                var favoritesOnPosts = await _context.Favorites
                    .Where(f => userPostIds.Contains(f.PostId))
                    .ToListAsync();
                _context.Favorites.RemoveRange(favoritesOnPosts);

                // 6. Delete reports on user's posts
                var reportsOnPosts = await _context.Reports
                    .Where(r => userPostIds.Contains(r.PostId))
                    .ToListAsync();
                _context.Reports.RemoveRange(reportsOnPosts);

                // 7. Delete user's posts
                var posts = await _context.Posts
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
                _context.Posts.RemoveRange(posts);
            }

            // 8. Delete likes made by this user (on other posts)
            var userLikes = await _context.Likes
                .Where(l => l.UserId == userId)
                .ToListAsync();
            _context.Likes.RemoveRange(userLikes);

            // 9. Delete comments made by this user (on other posts)
            var userComments = await _context.Comments
                .Where(c => c.UserId == userId)
                .ToListAsync();
            _context.Comments.RemoveRange(userComments);

            // 10. Delete favorites made by this user
            var userFavorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .ToListAsync();
            _context.Favorites.RemoveRange(userFavorites);

            // 11. Delete reports made by this user
            var userReports = await _context.Reports
                .Where(r => r.UserId == userId)
                .ToListAsync();
            _context.Reports.RemoveRange(userReports);

            // 12. Delete user's stories
            var userStories = await _context.Stories
                .Where(s => s.UserId == userId)
                .ToListAsync();
            _context.Stories.RemoveRange(userStories);

            // Note: We do NOT delete FriendRequests and Friendships here anymore 
            // to preserve the "User unavailable" context in lists, but we could if needed.
            // For now, let's keep them as the user record still exists.

            // 13. Soft delete the user
            user.IsDeleted = true;
            user.Name = "User unavailable";
            user.ProfilePictureUrl = null;
            user.Bio = "This account has been deleted.";
            
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // ==================== Analytics ====================
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalPostsCountAsync()
        {
            return await _context.Posts.Where(p => !p.IsDeleted).CountAsync();
        }

        public async Task<int> GetTotalReportsCountAsync()
        {
            return await _context.Reports.CountAsync();
        }


        public async Task<List<Post>> GetTopPostsAsync(int count = 10)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Likes.Count)
                .Take(count)
                .ToListAsync();
        }

        public Task<int> GetOnlineUsersCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task ApprovePostAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}