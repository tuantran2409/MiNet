using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static readonly ConcurrentDictionary<int, HashSet<string>> _onlineUsers = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                bool becameOnline = false;
                _onlineUsers.AddOrUpdate(userId.Value,
                    _ =>
                    {
                        becameOnline = true;
                        return new HashSet<string> { Context.ConnectionId };
                    },
                    (_, connections) =>
                    {
                        lock (connections)
                        {
                            connections.Add(Context.ConnectionId);
                        }
                        return connections;
                    });

                if (becameOnline)
                {
                    await Clients.Others.SendAsync("UserStatusChanged", new { userId = userId.Value, isOnline = true });
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                bool becameOffline = false;
                if (_onlineUsers.TryGetValue(userId.Value, out var connections))
                {
                    lock (connections)
                    {
                        connections.Remove(Context.ConnectionId);
                        if (connections.Count == 0)
                        {
                            becameOffline = true;
                        }
                    }
                    if (becameOffline)
                    {
                        _onlineUsers.TryRemove(userId.Value, out _);
                        await Clients.Others.SendAsync("UserStatusChanged", new { userId = userId.Value, isOnline = false });
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public Task<bool> CheckUserOnline(int targetUserId)
        {
            bool isOnline = _onlineUsers.ContainsKey(targetUserId);
            return Task.FromResult(isOnline);
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }

        public async Task SendTyping(int conversationId, bool isTyping)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return;

            var userName = Context.User?.Identity?.Name ?? "User";
            await Clients.OthersInGroup($"Conversation_{conversationId}").SendAsync("UserTyping", new
            {
                conversationId,
                userId = userId.Value,
                userName,
                isTyping
            });
        }

        public async Task SendMessage(int conversationId, string content, string? fileUrl, int type)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return;

            var messageType = (MessageType)type;
            var message = await _chatService.SaveMessageAsync(conversationId, userId.Value, content, messageType, fileUrl);

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

        private int? GetCurrentUserId()
        {
            var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
                return userId;
            return null;
        }
    }
}
