using MiNet.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace MiNet.Controllers
{
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
