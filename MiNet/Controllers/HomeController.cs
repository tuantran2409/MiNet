using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Data;
using MiNet.Data.Helpers;
using MiNet.Data.Models;
using MiNet.ViewModels.Home;

namespace MiNet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //get the loggedin User Id
            int loggedInUserId = 1;

            var allPosts = await _context.Posts
                .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && n.Reports.Count < 5 && !n.IsDeleted)
                .Include(u => u.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(u => u.User)
                .Include(n => n.Reports)
                .OrderByDescending(d => d.DateCreated)
                .ToListAsync();

            return View(allPosts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post) 
        {
            //get the logged in user
            int loggedInUser = 1;

            //create a new post
            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.Now,
                DateUpdate = DateTime.Now,
                ImageUrl = "",
                NrOfReports = 0,
                UserId = loggedInUser
            };

            //check and save an image
            if (post.Image != null && post.Image.Length > 0)
            {
                String rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (post.Image.ContentType.Contains("image"))
                {
                    String rootFolderPathImages = Path.Combine(rootFolderPath, "images/posts");
                    Directory.CreateDirectory(rootFolderPathImages);

                    String fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.Image.FileName);
                    String filePath = Path.Combine(rootFolderPathImages, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await post.Image.CopyToAsync(stream);

                    //set the url to the newPost object
                    newPost.ImageUrl = "/images/posts/" + fileName;
                }
            }


            //add the post to the database
            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();

            //find and store the hashtags
            var postHashtags = HashtagHelper.GetHashtags(post.Content);
            foreach(var hashTag in postHashtags)
            {
                var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(n => n.Name == hashTag);
                if(hashtagDb != null)
                {
                    hashtagDb.Count += 1;
                    hashtagDb.DateUpdate = DateTime.Now;

                    _context.Hashtags.Update(hashtagDb);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var newHashtag = new Hashtag()
                    {
                        Name = hashTag,
                        Count = 1,
                        DateCreated = DateTime.Now,
                        DateUpdate  = DateTime.Now
                    };

                    await _context.Hashtags.AddAsync(newHashtag);
                    await _context.SaveChangesAsync();
                }
            }
            //redirect to the index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostLike(PostLikeVM postLikeVM)
        {
            //get the logged in user
            int loggedInUserId = 1;

            //check if the user has already liked the post
            var like = await _context.Likes
                .Where(l => l.PostId ==  postLikeVM.PostId && l.UserId == loggedInUserId)
                .FirstOrDefaultAsync();

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postLikeVM.PostId,
                    UserId = loggedInUserId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            //get the logged in user
            int loggedInUserId = 1;

            //check if the user has already liked the post
            var favorite = await _context.Favorites
                .Where(l => l.PostId == postFavoriteVM.PostId && l.UserId == loggedInUserId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postFavoriteVM.PostId,
                    UserId = loggedInUserId
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibility(PostVisibilityVM postVisibilityVM)
        {
            //get the logged in user
            int loggedInUserId = 1;

            //get post by id and loggedIn User Id
            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postVisibilityVM.PostId && l.UserId == loggedInUserId);

            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM) 
        {
            //get the logged in user
            int loggedInUserId = 1;

            //create a comment object
            var newComment = new Comment()
            {
                UserId = loggedInUserId,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now
            };
            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostReport(PostReportVM postReportVM)
        {
            int loggedInUserId = 1;

            //Creat a post object
            var newReport = new Report()
            {
                UserId = loggedInUserId,
                PostId = postReportVM.PostId,
                DateCreated = DateTime.UtcNow,
            };
            await _context.Reports.AddAsync(newReport);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {
            var commentDb = await _context.Comments.FirstOrDefaultAsync(c => c.Id == removeCommentVM.CommentId);

            if(commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostRemove(PostRemoveVM postRemoveVM)
        {
            var postDb = await _context.Posts.FirstOrDefaultAsync(c => c.Id == postRemoveVM.PostId);

            if (postDb != null)
            {
                postDb.IsDeleted = true;
                _context.Posts.Update(postDb);
                await _context.SaveChangesAsync();

                //Update hashtag
                var postHashtags = HashtagHelper.GetHashtags(postDb.Content);
                foreach(var hashTag in postHashtags)
                {
                    var hashtagDb = await _context.Hashtags.FirstOrDefaultAsync(n => n.Name == hashTag);
                    if(hashtagDb != null)
                    {
                        hashtagDb.Count -= 1;
                        hashtagDb.DateUpdate = DateTime.Now;

                        _context.Hashtags.Update(hashtagDb);
                        await _context.SaveChangesAsync();
                    }
                }

            }

            return RedirectToAction("Index");
        }
    }
}
