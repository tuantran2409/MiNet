using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Controllers.Base;
using MiNet.Data;
using MiNet.Data.Services;
using MiNet.ViewModels.Explore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize]
    public class ExploreController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly IPostsService _postsService;

        public ExploreController(AppDbContext context, IPostsService postsService)
        {
            _context = context;
            _postsService = postsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? tag)
        {
            var vm = new ExploreVM
            {
                SelectedTag = tag
            };

            var currentUserId = GetUserId();
            var blockedUserIds = new List<int>();
            if (currentUserId.HasValue)
            {
                blockedUserIds = await _context.UserBlocks
                    .AsNoTracking()
                    .Where(b => b.BlockerId == currentUserId.Value || b.BlockedId == currentUserId.Value)
                    .Select(b => b.BlockerId == currentUserId.Value ? b.BlockedId : b.BlockerId)
                    .ToListAsync();
            }

            // Top trending hashtags
            vm.TrendingTags = await _context.Hashtags
                .AsNoTracking()
                .OrderByDescending(h => h.Count)
                .Take(12)
                .ToListAsync();

            // Popular creators (users with most posts, excluding blocked users)
            vm.PopularCreators = await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted && !blockedUserIds.Contains(u.Id) && u.Posts.Any())
                .OrderByDescending(u => u.Posts.Count)
                .Take(6)
                .ToListAsync();

            // Posts: if tag is selected, filter by tag, otherwise get top engagement posts
            if (!string.IsNullOrWhiteSpace(tag))
            {
                var cleanTag = tag.Trim().TrimStart('#').ToLower();
                vm.Posts = await _context.Posts
                    .AsNoTracking()
                    .Where(p => !p.IsDeleted && !p.IsPrivate && !blockedUserIds.Contains(p.UserId) && p.NrOfReports < 5 && p.Content.ToLower().Contains("#" + cleanTag))
                    .Include(p => p.User)
                    .Include(p => p.Likes)
                    .Include(p => p.Comments)
                    .OrderByDescending(p => p.DateCreated)
                    .Take(30)
                    .ToListAsync();
            }
            else
            {
                vm.Posts = (await _postsService.GetExplorePostsAsync(30))
                    .Where(p => !blockedUserIds.Contains(p.UserId))
                    .ToList();
            }

            return View(vm);
        }
    }
}
