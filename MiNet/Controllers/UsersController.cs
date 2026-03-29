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

        public UsersController(IUsersService usersService, UserManager<User> userManager)
        {
            _userService = usersService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var userPosts = await _userService.GetUserPosts(userId);
            var userProfileVM = new GetUserProfileVM()
            {
                User = user,
                Posts = userPosts
            };

            return View(userProfileVM);

        }
    }
}
