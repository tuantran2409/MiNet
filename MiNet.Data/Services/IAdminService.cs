using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public interface IAdminService
    {
        // Dashboard Statistics
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();

        // Reported Posts Management
        Task<List<Post>> GetReportedPostsAsync();
        Task<List<Report>> GetReportsForPostAsync(int postId);
        Task ApproveReportAsync(int postId);
        Task RejectReportAsync(int postId);

        // User Management
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task LockUserAsync(int userId);
        Task UnlockUserAsync(int userId);
        Task DeleteUserAsync(int userId);

        // Analytics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalPostsCountAsync();
        Task<int> GetTotalReportsCountAsync();
        Task<int> GetOnlineUsersCountAsync();
        Task<List<Post>> GetTopPostsAsync(int count = 10);
    }

    // ViewModel cho Dashboard
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalReports { get; set; }
        public int OnlineUsers { get; set; }
        public List<Post> ReportedPosts { get; set; } = new List<Post>();
        public List<Post> TopPosts { get; set; } = new List<Post>();
        public int NewUsersThisMonth { get; set; }
        public int NewPostsThisMonth { get; set; }
    }
}