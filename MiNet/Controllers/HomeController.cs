using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.Data.Helpers.Enums;
using MiNet.Data.Models;
using MiNet.ViewModels.Home;
using MiNet.Data.Services;

namespace MiNet.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;

        public HomeController(ILogger<HomeController> logger,
            IPostsService postsService,
            IHashtagsService hashtagsService,
            IFilesService filesService)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
        }

        public async Task<IActionResult> Index()
        {
            //get the loggedin User Id
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var allPosts = await _postsService.GetAllPostsAsync(loggedInUserId.Value);

            return View(allPosts);
        }

        public async Task<IActionResult> Details(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post) 
        {
            //get the logged in user
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            //get the upload img url
            var imageUploadPath = await _filesService.UploadImageAsync(post.Image,ImageFileType.PostImage);

            //create a new post
            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.Now,
                DateUpdate = DateTime.Now,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = loggedInUserId.Value
            };

            //add the post to the database
            await _postsService.CreatePostAsync(newPost);

            //find and store the hashtags
            await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content);

            //redirect to the index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePostLike(PostLikeVM postLikeVM)
        {
            //get the loggedIn user id
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            await _postsService.TogglePostLikeAsync(postLikeVM.PostId, loggedInUserId.Value);

            var post = await _postsService.GetPostByIdAsync(postLikeVM.PostId);
            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            //get the logged in user
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId, loggedInUserId.Value);

            var post = await _postsService.GetPostByIdAsync(postFavoriteVM.PostId);
            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibility(PostVisibilityVM postVisibilityVM)
        {
            //get the logged in user
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            await _postsService.TogglePostVisibilityAsync(postVisibilityVM.PostId, loggedInUserId.Value);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM) 
        {
            //get the logged in user
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            //create a comment object
            var newComment = new Comment()
            {
                UserId = loggedInUserId.Value,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            await _postsService.AddPostCommentAsync(newComment);

            var post = await _postsService.GetPostByIdAsync(postCommentVM.PostId);
            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            await _postsService.ReportPostAsync(postReportVM.PostId, loggedInUserId.Value);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {
            await _postsService.RemovePostCommentAsync(removeCommentVM.CommentId);

            var post = await _postsService.GetPostByIdAsync(removeCommentVM.PostId);
            return PartialView("Home/_Post", post);
        }

        [HttpPost]
        public async Task<IActionResult> PostRemove(PostRemoveVM postRemoveVM)
        {
            var postRemoved = await _postsService.RemovePostAsync(postRemoveVM.PostId);
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content);

            return RedirectToAction("Index");
        }
    }
}
