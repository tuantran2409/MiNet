using MiNet.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiNet.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly IPostsService _postsService;

        public FavoritesController(IPostsService postService)
        {
            _postsService = postService;
        }

        public async Task<IActionResult> Index()
        {
            int loggedInUserId = 1;
            var myFavoritePosts = await _postsService.GetAllFavoritedPostsAsync(loggedInUserId);

            return View(myFavoritePosts);
        }
    }
}
