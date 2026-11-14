using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.ViewModels.Users;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using MiNet.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class UsersController : BaseController
    {
        private readonly IUsersService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IFriendsService _friendsService;
        private readonly IFollowService _followService;
        private readonly IPostsService _postsService;
        private readonly IStoriesService _storiesService;

        public UsersController(
            IUsersService usersService,
            UserManager<User> userManager,
            IFriendsService friendsService,
            IFollowService followService,
            IPostsService postsService,
            IStoriesService storiesService)
        {
            _userService = usersService;
            _userManager = userManager;
            _friendsService = friendsService;
            _followService = followService;
            _postsService = postsService;
            _storiesService = storiesService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            var currentUserId = GetUserId() ?? 0;
            bool isSelf = (currentUserId == userId);

            var userPosts = await _userService.GetUserPosts(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var friendships = await _friendsService.GetFriendsAsync(userId);
            var friendsList = friendships.Select(f => f.SenderId == userId ? f.Receiver : f.Sender).ToList();

            int followersCount = await _followService.GetFollowersCountAsync(userId);
            int followingCount = await _followService.GetFollowingCountAsync(userId);
            bool isFollowing = !isSelf && await _followService.IsFollowingAsync(currentUserId, userId);
            bool isFriend = !isSelf && friendships.Any(f => f.SenderId == currentUserId || f.ReceiverId == currentUserId);

            List<Post> savedPosts = new List<Post>();
            if (isSelf)
            {
                savedPosts = await _postsService.GetAllFavoritedPostsAsync(currentUserId);
            }

            var highlights = await _storiesService.GetUserHighlightsAsync(userId);

            var userProfileVM = new GetUserProfileVM
            {
                User = user,
                Posts = userPosts,
                SavedPosts = savedPosts,
                Highlights = highlights,
                Roles = roles,
                Friends = friendsList,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                IsFollowing = isFollowing,
                IsFriend = isFriend,
                IsSelf = isSelf
            };

            return View(userProfileVM);
        }
    }
}
