using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNet.Data.Services;
using System.Security.Claims;

namespace MiNet.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUsersService _usersService;

        public ChatController(IChatService chatService, IUsersService usersService)
        {
            _chatService = chatService;
            _usersService = usersService;
        }

       
        public async Task<IActionResult> Index(int? friendId)
        {
        
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (currentUserId == 0) return RedirectToAction("Login", "Authentication");

           
            var friends = await _usersService.GetFriendsAsync(currentUserId);

            
            System.Diagnostics.Debug.WriteLine($"=== Số bạn bè: {friends.Count} ===");

           
            ViewBag.Friends = friends;

           
            if (friendId.HasValue)
            {
                var messages = await _chatService.GetMessagesBetweenUsersAsync(currentUserId, friendId.Value);
                ViewBag.Messages = messages;
                ViewBag.CurrentFriendId = friendId.Value;

                
                await _chatService.MarkMessagesAsReadAsync(friendId.Value, currentUserId);
            }

            return View();
        }

        
        [HttpGet]
        public async Task<IActionResult> GetMessages(int friendId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (currentUserId == 0) return Unauthorized();

            var messages = await _chatService.GetMessagesBetweenUsersAsync(currentUserId, friendId);

        
            await _chatService.MarkMessagesAsReadAsync(friendId, currentUserId);

            return Ok(messages);
        }


        [HttpGet]
        public async Task<IActionResult> GetRecentChats()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (currentUserId == 0) return Unauthorized();

            var recentChats = await _chatService.GetRecentChatsAsync(currentUserId);

            return Ok(recentChats);
        }
    }
}