using MiNet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiNet.ViewComponents
{
    public class StoriesViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        public StoriesViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var allStories = await _context.Stories
                .Where(n => n.DateCreated >= DateTime.Now.AddHours(-24))
                .Include(s => s.User)
                .ToListAsync();

            return View(allStories);
        }
    }
}
