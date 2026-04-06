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

        public async Task<Message> SaveMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesBetweenUsersAsync(int user1Id, int user2Id)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                            (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

     
        public async Task MarkMessagesAsReadAsync(int senderId, int receiverId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatUserDto>> GetRecentChatsAsync(int userId)
        {
            var recentMessages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault(),
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                })
                .OrderByDescending(x => x.LastMessage.Timestamp)
                .Take(20)
                .ToListAsync();

            var result = new List<ChatUserDto>();

            foreach (var item in recentMessages)
            {
                var user = await _context.Users.FindAsync(item.UserId);
                if (user != null)
                {
                    result.Add(new ChatUserDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName ?? "",
                        Name = user.Name ?? "",
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        LastMessage = item.LastMessage?.Content ?? "",
                        LastMessageTime = item.LastMessage?.Timestamp ?? DateTime.Now,
                        UnreadCount = item.UnreadCount
                    });
                }
            }

            return result;
        }
    }
}