using Microsoft.AspNetCore.Mvc;
using MiNet.Data;
using Microsoft.EntityFrameworkCore;

namespace MiNet.ViewComponents
{
    public class HashtagsViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public HashtagsViewComponent(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var OneWeekAgoNow = DateTime.Now.AddDays(-7);

            var top3Hashtags = await _context.Hashtags
                .Where(h => h.DateCreated >= OneWeekAgoNow)
                .OrderByDescending(n => n.DateCreated)
                .Take(3)
                .ToListAsync();

            return View(top3Hashtags);
        }
    }
}
