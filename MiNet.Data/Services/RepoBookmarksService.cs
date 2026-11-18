using Microsoft.EntityFrameworkCore;
using MiNet.Data.Dtos;
using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public class RepoBookmarksService : IRepoBookmarksService
    {
        private readonly AppDbContext _context;

        public RepoBookmarksService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleAsync(int userId, GitHubRepoDto repo)
        {
            var existing = await _context.RepoBookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.RepoFullName == repo.FullName);

            if (existing != null)
            {
                _context.RepoBookmarks.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            var bookmark = new RepoBookmark
            {
                UserId = userId,
                RepoFullName = repo.FullName,
                RepoHtmlUrl = repo.HtmlUrl,
                OwnerAvatarUrl = repo.OwnerAvatarUrl,
                Description = Truncate(repo.Description, 1000),
                Language = repo.Language,
                Stars = repo.Stars,
                DateCreated = DateTime.Now
            };

            await _context.RepoBookmarks.AddAsync(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsBookmarkedAsync(int userId, string repoFullName)
        {
            return await _context.RepoBookmarks
                .AnyAsync(b => b.UserId == userId && b.RepoFullName == repoFullName);
        }

        public async Task<List<RepoBookmark>> GetUserBookmarksAsync(int userId)
        {
            return await _context.RepoBookmarks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateCreated)
                .ToListAsync();
        }

        public async Task<bool> RemoveAsync(int userId, string repoFullName)
        {
            var bookmark = await _context.RepoBookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.RepoFullName == repoFullName);

            if (bookmark == null) return false;

            _context.RepoBookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
