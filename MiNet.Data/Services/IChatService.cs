using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public interface IChatService
    {
        Task<Message> SaveMessageAsync(Message message);
        Task<List<Message>> GetMessagesBetweenUsersAsync(int user1Id, int user2Id);
        Task MarkMessagesAsReadAsync(int senderId, int receiverId); 
        Task<List<ChatUserDto>> GetRecentChatsAsync(int userId);
    }

    public class ChatUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}