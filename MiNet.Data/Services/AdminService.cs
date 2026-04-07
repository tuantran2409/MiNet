using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNet.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MiNet.Data.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        public AdminService(AppDbContext context)
        {
            _context = context;
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
            var user = await _context.Users
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                // Xóa tất cả bài viết của người dùng
                foreach (var post in user.Posts)
                {
                    post.IsDeleted = true;
                }

                user.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
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