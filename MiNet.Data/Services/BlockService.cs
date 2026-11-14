using Microsoft.EntityFrameworkCore;
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class BlockService : IBlockService
    {
        private readonly AppDbContext _context;

        public BlockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> BlockUserAsync(int blockerId, int blockedId)
        {
            if (blockerId == blockedId) return false;

            var existing = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

            if (existing != null) return true; // Already blocked

            var block = new UserBlock
            {
                BlockerId = blockerId,
                BlockedId = blockedId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.UserBlocks.AddAsync(block);

            // Automatically remove any friendships between the two
            var friendships = await _context.Friendships
                .Where(f => (f.SenderId == blockerId && f.ReceiverId == blockedId) || (f.SenderId == blockedId && f.ReceiverId == blockerId))
                .ToListAsync();
            if (friendships.Any())
            {
                _context.Friendships.RemoveRange(friendships);
            }

            // Automatically remove any friend requests between the two
            var requests = await _context.FriendRequests
                .Where(r => (r.SenderId == blockerId && r.ReceiverId == blockedId) || (r.SenderId == blockedId && r.ReceiverId == blockerId))
                .ToListAsync();
            if (requests.Any())
            {
                _context.FriendRequests.RemoveRange(requests);
            }

            // Automatically remove 2-way follows between the two
            var follows = await _context.UserFollows
                .Where(f => (f.FollowerId == blockerId && f.FollowingId == blockedId) || (f.FollowerId == blockedId && f.FollowingId == blockerId))
                .ToListAsync();
            if (follows.Any())
            {
                _context.UserFollows.RemoveRange(follows);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnblockUserAsync(int blockerId, int blockedId)
        {
            var block = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);

            if (block != null)
            {
                _context.UserBlocks.Remove(block);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> IsBlockedAsync(int userA, int userB)
        {
            if (userA == userB) return false;

            return await _context.UserBlocks
                .AnyAsync(b => (b.BlockerId == userA && b.BlockedId == userB) || (b.BlockerId == userB && b.BlockedId == userA));
        }

        public async Task<List<int>> GetBlockedUserIdsAsync(int userId)
        {
            // Users that 'userId' blocked OR users that blocked 'userId'
            var blockedByMe = await _context.UserBlocks
                .Where(b => b.BlockerId == userId)
                .Select(b => b.BlockedId)
                .ToListAsync();

            var blockingMe = await _context.UserBlocks
                .Where(b => b.BlockedId == userId)
                .Select(b => b.BlockerId)
                .ToListAsync();

            return blockedByMe.Union(blockingMe).Distinct().ToList();
        }
    }
}

