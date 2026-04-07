using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Data;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Models;
using MiNet.Data.Services;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public AdminController(IAdminService adminService, UserManager<User> userManager, AppDbContext context)
        {
            _adminService = adminService;
            _userManager = userManager;
            _context = context;  
        }

        // ==================== Dashboard ====================
        public async Task<IActionResult> Index()
        {
            var dashboardStats = await _adminService.GetDashboardStatsAsync();
            return View(dashboardStats);
        }

        // ==================== Reported Posts ====================
        public async Task<IActionResult> ReportedPosts()
        {
            var reportedPosts = await _adminService.GetReportedPostsAsync();
            return View(reportedPosts);
        }

        public async Task<IActionResult> PostDetails(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return NotFound();

            var reports = await _adminService.GetReportsForPostAsync(postId);
            ViewBag.Reports = reports;
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReport(int postId)
        {
            await _adminService.ApproveReportAsync(postId);
            TempData["SuccessMessage"] = "✓ Bài viết đã bị xóa thành công!";
            return RedirectToAction("ReportedPosts");
        }

        [HttpPost]
        public async Task<IActionResult> RejectReport(int postId)
        {
            await _adminService.RejectReportAsync(postId);
            TempData["SuccessMessage"] = "✓ Báo cáo đã bị từ chối!";
            return RedirectToAction("ReportedPosts");
        }

        // ==================== User Management ====================
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Posts) 
                .OrderByDescending(u => u.Id)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Posts)     
                .Include(u => u.Comments)   
                .Include(u => u.Likes)    
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(int userId)
        {
            await _adminService.LockUserAsync(userId);
            return RedirectToAction("UserDetails", new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> UnlockUser(int userId)
        {
            await _adminService.UnlockUserAsync(userId);
            return RedirectToAction("UserDetails", new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            await _adminService.DeleteUserAsync(userId);
            TempData["SuccessMessage"] = "✓ Người dùng đã bị xóa!";
            return RedirectToAction("Users");
        }

        // ==================== Analytics ====================
        public async Task<IActionResult> Analytics()
        {
            var stats = new
            {
                TotalUsers = await _adminService.GetTotalUsersCountAsync(),
                TotalPosts = await _adminService.GetTotalPostsCountAsync(),
                TotalReports = await _adminService.GetTotalReportsCountAsync(),
                TopPosts = await _adminService.GetTopPostsAsync(10)
            };
            ViewBag.stats = stats;
            return View();
        }

    }
}