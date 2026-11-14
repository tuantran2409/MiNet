using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;
        private readonly INotificationsService _notificationsService;

        public FollowController(IFollowService followService, INotificationsService notificationsService)
        {
            _followService = followService;
            _notificationsService = notificationsService;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollow(int targetUserId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return Unauthorized();

            if (loggedInUserId.Value == targetUserId)
                return BadRequest("Cannot follow yourself.");

            bool isNowFollowing = await _followService.ToggleFollowAsync(loggedInUserId.Value, targetUserId);
            int followersCount = await _followService.GetFollowersCountAsync(targetUserId);

            if (isNowFollowing)
            {
                var userName = GetUserFullName();
                await _notificationsService.AddNewNotificationAsync(targetUserId, NotificationType.FriendRequest, $"{userName} started following you", null);
            }

            return Json(new
            {
                success = true,
                isFollowing = isNowFollowing,
                followersCount = followersCount
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCounts(int userId)
        {
            int followersCount = await _followService.GetFollowersCountAsync(userId);
            int followingCount = await _followService.GetFollowingCountAsync(userId);

            return Json(new
            {
                followersCount = followersCount,
                followingCount = followingCount
            });
        }
    }
}
