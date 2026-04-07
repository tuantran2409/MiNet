using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MiNet.Controllers.Base;
using MiNet.Data.Models;
using MiNet.Data.Services;
using MiNet.ViewModels.Chat;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly IFilesService _filesService;
        private readonly IUsersService _usersService;
        private readonly UserManager<User> _userManager;

        public ChatController(IChatService chatService, IFilesService filesService, IUsersService usersService, UserManager<User> userManager)
        {
            _chatService = chatService;
            _filesService = filesService;
            _usersService = usersService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var conversations = await _chatService.GetUserConversationsAsync(userId.Value);
            var viewModel = new ChatListVM
            {
                Conversations = conversations,
                CurrentUserId = userId.Value
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Message(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var conversation = await _chatService.GetConversationByIdAsync(id, userId.Value);
            if (conversation == null) return NotFound();

            var messages = await _chatService.GetMessageHistoryAsync(id);
            var viewModel = new ConversationDetailsVM
            {
                Conversation = conversation,
                Messages = messages,
                CurrentUserId = userId.Value
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> StartPrivateChat(int targetUserId)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            // Protection: Users cannot message Admins
            var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
            if (targetUser == null) return NotFound();

            var isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
            var isCurrentAdmin = User.IsInRole("Admin");

            if (isTargetAdmin && !isCurrentAdmin)
            {
                return Forbid(); // Regular users can't message admins
            }

            // Check if existing
            var existing = await _chatService.GetPrivateConversationAsync(userId.Value, targetUserId);
            if (existing != null)
                return RedirectToAction("Message", new { id = existing.Id });

            // Create new
            var memberIds = new List<int> { userId.Value, targetUserId };
            var conversation = await _chatService.CreateConversationAsync(memberIds);

            return RedirectToAction("Message", new { id = conversation.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(string title, int[] memberIds)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            var allMemberIds = memberIds.ToList();
            if (!allMemberIds.Contains(userId.Value))
                allMemberIds.Add(userId.Value);

            var conversation = await _chatService.CreateConversationAsync(allMemberIds, title);

            return RedirectToAction("Message", new { id = conversation.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return RedirectToLogin();

            await _chatService.LeaveConversationAsync(userId.Value, id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File size exceeds 10MB limit.");

            try
            {
                var fileUrl = await _filesService.UploadFileAsync(file, "attachments/chat");
                return Ok(new { url = fileUrl });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
