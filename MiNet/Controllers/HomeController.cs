using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Data;
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
            var allPosts = await _context.Posts
                .Include(u => u.User)
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
                    String rootFolderPathImages = Path.Combine(rootFolderPath, "images/uploaded");
                    Directory.CreateDirectory(rootFolderPathImages);

                    String fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.Image.FileName);
                    String filePath = Path.Combine(rootFolderPathImages, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await post.Image.CopyToAsync(stream);

                    //set the url to the newPost object
                    newPost.ImageUrl = "/images/uploaded/" + fileName;
                }
            }


            //add the post to the database
            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();

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
    }
}
