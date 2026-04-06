using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MiNet.Data;
using MiNet.Data.Models;
using MiNet.Data.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MiNet.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly AppDbContext _context;

        public ChatHub(IChatService chatService, AppDbContext context)
        {
            _chatService = chatService;
            _context = context;
        }

        // Gửi tin nhắn văn bản
        public async Task SendMessage(int receiverId, string message)
        {
            var senderId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (senderId == 0) return;

            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.Name ?? sender?.UserName ?? "Người dùng";

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                Timestamp = DateTime.Now,
                IsRead = false,
                ImageUrl = null,
                FileUrl = null,
                FileName = null,
                FileSize = null
            };

            await _chatService.SaveMessageAsync(newMessage);

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
            {
                id = newMessage.Id,
                senderId = senderId,
                receiverId = receiverId,
                content = message,
                imageUrl = (string?)null,
                fileUrl = (string?)null,
                fileName = (string?)null,
                fileSize = (long?)null,
                timestamp = newMessage.Timestamp,
                isRead = false,
                senderName = senderName
            });

            await Clients.Caller.SendAsync("MessageSent", new
            {
                id = newMessage.Id,
                senderId = senderId,
                receiverId = receiverId,
                content = message,
                imageUrl = (string?)null,
                fileUrl = (string?)null,
                fileName = (string?)null,
                fileSize = (long?)null,
                timestamp = newMessage.Timestamp,
                isRead = false,
                senderName = senderName
            });
        }

        // Gửi tin nhắn có ảnh
        public async Task SendMessageWithImage(int receiverId, string message, string imageUrl)
        {
            var senderId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (senderId == 0) return;

            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.Name ?? sender?.UserName ?? "Người dùng";

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message ?? "",
                ImageUrl = imageUrl,
                Timestamp = DateTime.Now,
                IsRead = false,
                FileUrl = null,
                FileName = null,
                FileSize = null
            };

            await _chatService.SaveMessageAsync(newMessage);

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", new
            {
                id = newMessage.Id,
                senderId = senderId,
                receiverId = receiverId,
                content = message ?? "",
                imageUrl = imageUrl,
                fileUrl = (string?)null,
                fileName = (string?)null,
                fileSize = (long?)null,
                timestamp = newMessage.Timestamp,
                isRead = false,
                senderName = senderName
            });

            await Clients.Caller.SendAsync("MessageSent", new
            {
                id = newMessage.Id,
                senderId = senderId,
                receiverId = receiverId,
                content = message ?? "",
                imageUrl = imageUrl,
                fileUrl = (string?)null,
                fileName = (string?)null,
                fileSize = (long?)null,
                timestamp = newMessage.Timestamp,
                isRead = false,
                senderName = senderName
            });
        }

        // Gửi file (PDF, Word, Excel, ZIP, ...)
        public async Task SendFile(int receiverId, string fileUrl, string fileName, long fileSize)
        {
            var senderId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (senderId == 0) return;

            var sender = await _context.Users.FindAsync(senderId);
            var senderName = sender?.Name ?? sender?.UserName ?? "Người dùng";

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = "",
                FileUrl = fileUrl,
                FileName = fileName,
                FileSize = fileSize,
                Timestamp = DateTime.Now,
                IsRead = false,
                ImageUrl = null
            };

            await _chatService.SaveMessageAsync(newMessage);

            await Clients.User(receiverId.ToString()).SendAsync("ReceiveFile", new
            {
                id = newMessage.Id,
                senderId = senderId,
                receiverId = receiverId,
                fileUrl = fileUrl,
                fileName = fileName,
                fileSize = fileSize,
                timestamp = newMessage.Timestamp,
                isRead = false,
                senderName = senderName
            });

            await Clients.Caller.SendAsync("FileSent", new
            {
                id = newMessage.Id,
                senderId = senderId,
                receiverId = receiverId,
                fileUrl = fileUrl,
                fileName = fileName,
                fileSize = fileSize,
                timestamp = newMessage.Timestamp,
                isRead = false,
                senderName = senderName
            });
        }

        // Đánh dấu tất cả tin nhắn từ người gửi đã được đọc
        public async Task MarkMessagesAsRead(int senderId)
        {
            var currentUserId = int.Parse(Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId == 0) return;

            await _chatService.MarkMessagesAsReadAsync(senderId, currentUserId);

            var currentUser = await _context.Users.FindAsync(currentUserId);
            var readerName = currentUser?.Name ?? currentUser?.UserName ?? "Bạn";

            await Clients.User(senderId.ToString()).SendAsync("MessagesRead", new
            {
                readerId = currentUserId,
                readerName = readerName,
                timestamp = DateTime.Now
            });
        }

        // Khi người dùng kết nối
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        // Khi người dùng ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}