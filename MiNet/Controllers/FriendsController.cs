using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using MiNet.ViewModels.Friends;
using MiNet.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class FriendsController : BaseController
    {
        public readonly IFriendsService _friendsService;
        private readonly INotificationsService _notificationsService;
        private readonly UserManager<User> _userManager;

        public FriendsController(IFriendsService friendsService, INotificationsService notificationsService, UserManager<User> userManager)
        {
            _friendsService = friendsService;
            _notificationsService = notificationsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (!userId.HasValue) RedirectToLogin();

            var friendsData = new FriendshipVM()
            {
                Friends = await _friendsService.GetFriendsAsync(userId.Value),
                FriendRequestsSent = await _sentFriendRequestsAsync(userId.Value), // This seems like it was a local call before? No, GetSentFriendRequestAsync
                FriendRequestsReceived = await _friendsService.GetReceivedFriendRequestAsync(userId.Value)
            };
            
            // Get Admin IDs to hide interaction buttons in view
            var admins = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
            ViewBag.AdminIds = admins.Select(a => a.Id).ToList();

            return View(friendsData);
        }

        private async Task<List<FriendRequest>> _sentFriendRequestsAsync(int userId)
        {
             return await _friendsService.GetSentFriendRequestAsync(userId);
        }

        [HttpPost]
        public async Task<IActionResult> SendFriendRequest(int receiverId)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue) RedirectToLogin();

            // Protection: Users cannot send requests to Admins
            var receiver = await _userManager.FindByIdAsync(receiverId.ToString());
            if (receiver != null && await _userManager.IsInRoleAsync(receiver, AppRoles.Admin))
            {
                return Forbid();
            }

            await _friendsService.SendRequestAsync(userId.Value, receiverId);
            await _notificationsService.AddNewNotificationAsync(receiverId, NotificationType.FriendRequest, userName, null);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFriendRequest(int requestId, string status)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue) RedirectToLogin();

            var request = await _friendsService.UpdateRequestAsync(requestId, status);

            if (status == FriendshipStatus.Accepted)
                await _notificationsService.AddNewNotificationAsync(request.SenderId, NotificationType.FriendRequestApproved, userName, null);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            await _friendsService.RemoveFriendAsync(friendshipId);
            return RedirectToAction("Index");
        }
    }
}
