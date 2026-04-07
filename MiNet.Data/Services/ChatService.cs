using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.Members.Any(m => m.UserId == userId))
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .Include(c => c.Messages.OrderByDescending(m => m.DateSent).Take(1))
                .OrderByDescending(c => c.Messages.Any() ? c.Messages.Max(m => m.DateSent) : c.DateCreated)
                .ToListAsync();
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId, int userId)
        {
            // Verify user is a member
            var isMember = await _context.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);

            if (!isMember) return null;

            return await _context.Conversations
                .Include(c => c.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<Conversation> CreateConversationAsync(List<int> memberUserIds, string? title = null)
        {
            var isGroup = memberUserIds.Count > 2 || !string.IsNullOrEmpty(title);

            var conversation = new Conversation
            {
                Title = title,
                IsGroup = isGroup,
                DateCreated = DateTime.UtcNow
            };

            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var members = memberUserIds.Select(userId => new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = userId,
                JoinedDate = DateTime.UtcNow
            }).ToList();

            await _context.ConversationMembers.AddRangeAsync(members);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<Message> SaveMessageAsync(int conversationId, int senderId, string content, MessageType type, string? fileUrl = null)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                Type = type,
                FileUrl = fileUrl,
                DateSent = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
            
            // Load sender info for UI
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();

            return message;
        }

        public async Task<List<Message>> GetMessageHistoryAsync(int conversationId, int skip = 0, int take = 50)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.DateSent)
                .Skip(skip)
                .Take(take)
                .OrderBy(m => m.DateSent)
                .ToListAsync();
        }

        public async Task<bool> LeaveConversationAsync(int userId, int conversationId)
        {
            var member = await _context.ConversationMembers
                .FirstOrDefaultAsync(cm => cm.ConversationId == conversationId && cm.UserId == userId);

            if (member == null) return false;

            _context.ConversationMembers.Remove(member);
            await _context.SaveChangesAsync();

            // If it was a 1-on-1 and someone leaves, or if no one is left in a group, 
            // the conversation persists but is hidden from the user who left.
            // If it's a group chat and no members are left, we could optionally delete it.
            var remainingMembers = await _context.ConversationMembers.AnyAsync(cm => cm.ConversationId == conversationId);
            if (!remainingMembers)
            {
                var conversation = await _context.Conversations.FindAsync(conversationId);
                if (conversation != null)
                {
                    _context.Conversations.Remove(conversation);
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<Conversation?> GetPrivateConversationAsync(int user1Id, int user2Id)
        {
            // Find a non-group conversation where both users are members
            return await _context.Conversations
                .Where(c => !c.IsGroup)
                .Where(c => c.Members.Any(m => m.UserId == user1Id) && c.Members.Any(m => m.UserId == user2Id))
                .FirstOrDefaultAsync();
        }
    }
}
