using Microsoft.EntityFrameworkCore;
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class UserInteractionService : IUserInteractionService
    {
        private readonly AppDbContext _context;

        public UserInteractionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task RecordInteractionAsync(int userId, int postId, InteractionType type, int dwellSeconds = 0)
        {
            if (userId <= 0 || postId <= 0) return;

            float weightScore = type switch
            {
                InteractionType.View => 1.0f + Math.Min(dwellSeconds * 0.1f, 2.0f),
                InteractionType.Like => 3.0f,
                InteractionType.Comment => 5.0f,
                InteractionType.Save => 7.0f,
                InteractionType.Share => 7.0f,
                _ => 1.0f
            };

            // Upsert or log interaction
            var existing = await _context.UserInteractions
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.PostId == postId && ui.InteractionType == type);

            if (existing != null)
            {
                existing.WeightScore = Math.Max(existing.WeightScore, weightScore);
                existing.DwellTimeSeconds = Math.Max(existing.DwellTimeSeconds, dwellSeconds);
                existing.CreatedAt = DateTime.UtcNow;
                _context.UserInteractions.Update(existing);
            }
            else
            {
                var newInteraction = new UserInteraction
                {
                    UserId = userId,
                    PostId = postId,
                    InteractionType = type,
                    DwellTimeSeconds = dwellSeconds,
                    WeightScore = weightScore,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserInteractions.AddAsync(newInteraction);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserInteractionsCountAsync(int userId)
        {
            return await _context.UserInteractions
                .AsNoTracking()
                .CountAsync(ui => ui.UserId == userId);
        }

        public async Task<List<string>> GetUserInterestsAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrWhiteSpace(user.InterestsJson))
                return new List<string>();

            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(user.InterestsJson);
                return list ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
