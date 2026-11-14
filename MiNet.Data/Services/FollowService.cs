using Microsoft.EntityFrameworkCore;
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;

        public FollowService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
        {
            if (followerId == followingId) return false;

            var existing = await _context.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existing != null)
            {
                _context.UserFollows.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // Now unfollowed
            }
            else
            {
                var follow = new UserFollow
                {
                    FollowerId = followerId,
                    FollowingId = followingId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserFollows.AddAsync(follow);
                await _context.SaveChangesAsync();
                return true; // Now followed
            }
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            if (followerId == followingId) return false;

            return await _context.UserFollows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<int> GetFollowersCountAsync(int userId)
        {
            return await _context.UserFollows
                .CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.UserFollows
                .CountAsync(f => f.FollowerId == userId);
        }

        public async Task<List<User>> GetFollowersAsync(int userId)
        {
            return await _context.UserFollows
                .Where(f => f.FollowingId == userId)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<List<User>> GetFollowingAsync(int userId)
        {
            return await _context.UserFollows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.Following)
                .ToListAsync();
        }
    }
}

