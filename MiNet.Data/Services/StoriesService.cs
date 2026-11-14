using Microsoft.EntityFrameworkCore;
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class StoriesService : IStoriesService
    {
        private readonly AppDbContext _context;

        public StoriesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Story>> GetAllStoriesAsync()
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            var allStories = await _context.Stories
                .Where(n => !n.IsDeleted && n.DateCreated >= cutoff)
                .Include(s => s.User)
                .Include(s => s.Views)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();

            return allStories;
        }

        public async Task<Story> CreateStoryAsync(Story story)
        {
            story.DateCreated = DateTime.UtcNow;
            await _context.Stories.AddAsync(story);
            await _context.SaveChangesAsync();
            return story;
        }

        public async Task<bool> RecordStoryViewAsync(int storyId, int viewerId)
        {
            var story = await _context.Stories.FirstOrDefaultAsync(s => s.Id == storyId);
            if (story == null) return false;

            var existing = await _context.StoryViews
                .FirstOrDefaultAsync(sv => sv.StoryId == storyId && sv.ViewerId == viewerId);

            if (existing == null)
            {
                var view = new StoryView
                {
                    StoryId = storyId,
                    ViewerId = viewerId,
                    ViewedAt = DateTime.UtcNow
                };
                await _context.StoryViews.AddAsync(view);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<List<User>> GetStoryViewersAsync(int storyId)
        {
            return await _context.StoryViews
                .Where(sv => sv.StoryId == storyId)
                .OrderByDescending(sv => sv.ViewedAt)
                .Select(sv => sv.Viewer)
                .ToListAsync();
        }

        public async Task<bool> HasUserViewedStoryAsync(int storyId, int userId)
        {
            return await _context.StoryViews
                .AnyAsync(sv => sv.StoryId == storyId && sv.ViewerId == userId);
        }
    }
}
