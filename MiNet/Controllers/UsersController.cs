using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.ViewModels.Users;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using MiNet.Data.Models;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class UsersController : BaseController
    {
        private readonly IUsersService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IFriendsService _friendsService;

        public UsersController(IUsersService usersService, UserManager<User> userManager, IFriendsService friendsService)
        {
            _userService = usersService;
            _userManager = userManager;
            _friendsService = friendsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var userPosts = await _userService.GetUserPosts(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var friendships = await _friendsService.GetFriendsAsync(userId);
            
            var friendsList = friendships.Select(f => f.SenderId == userId ? f.Receiver : f.Sender).ToList();

            var userProfileVM = new GetUserProfileVM()
            {
                User = user,
                Posts = userPosts,
                Roles = roles,
                Friends = friendsList
            };

            return View(userProfileVM);

        }
    }
}
