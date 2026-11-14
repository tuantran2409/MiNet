using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Controllers.Base;
using MiNet.Data;
using MiNet.ViewModels.Search;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize]
    public class SearchController : BaseController
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string tab = "all")
        {
            var query = (q ?? string.Empty).Trim();
            var vm = new SearchVM
            {
                Query = query,
                ActiveTab = tab.ToLower()
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(vm);
            }

            var lowerQuery = query.ToLower();
            string cleanTag = lowerQuery.StartsWith("#") ? lowerQuery.Substring(1) : lowerQuery;

            // Search Users
            if (vm.ActiveTab == "all" || vm.ActiveTab == "users")
            {
                vm.Users = await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted && (u.Name.ToLower().Contains(lowerQuery) || (u.UserName != null && u.UserName.ToLower().Contains(lowerQuery))))
                    .Take(20)
                    .ToListAsync();
            }

            // Search Posts
            if (vm.ActiveTab == "all" || vm.ActiveTab == "posts")
            {
                vm.Posts = await _context.Posts
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && !p.IsPrivate && p.NrOfReports < 5 && p.Content.ToLower().Contains(lowerQuery))
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                    .OrderByDescending(p => p.DateCreated)
                    .Take(20)
                    .ToListAsync();
            }

            // Search Hashtags
            if (vm.ActiveTab == "all" || vm.ActiveTab == "tags")
            {
                vm.Hashtags = await _context.Hashtags
                    .AsNoTracking()
                    .Where(h => h.Name.ToLower().Contains(cleanTag))
                    .OrderByDescending(h => h.Count)
                    .Take(15)
                    .ToListAsync();
            }

            return View(vm);
        }
    }
}
