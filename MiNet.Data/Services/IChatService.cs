using System.Collections.Generic;
using System.Threading.Tasks;
using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public interface IChatService
    {
        Task<List<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation?> GetConversationByIdAsync(int conversationId, int userId);
        Task<Conversation> CreateConversationAsync(List<int> memberUserIds, string? title = null);
        Task<Message> SaveMessageAsync(int conversationId, int senderId, string content, MessageType type, string? fileUrl = null);
        Task<List<Message>> GetMessageHistoryAsync(int conversationId, int skip = 0, int take = 50);
        Task<bool> LeaveConversationAsync(int userId, int conversationId);
        Task<Conversation?> GetPrivateConversationAsync(int user1Id, int user2Id);
    }
}
