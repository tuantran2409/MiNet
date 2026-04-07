using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MiNet.Data.Models;
using MiNet.Data.Services;

namespace MiNet.Data.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }

        public async Task SendMessage(int conversationId, string content, string? fileUrl, int type)
        {
            var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return;

            var messageType = (MessageType)type;
            var message = await _chatService.SaveMessageAsync(conversationId, userId, content, messageType, fileUrl);

            // Broadcast to the group
            await Clients.Group($"Conversation_{conversationId}").SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                senderName = message.Sender?.Name ?? "User unavailable",
                senderProfilePicture = message.Sender?.ProfilePictureUrl,
                content = message.Content,
                type = (int)message.Type,
                fileUrl = message.FileUrl,
                dateSent = message.DateSent.ToString("o"),
                isDeletedUser = message.Sender?.IsDeleted ?? false
            });
        }
    }
}
