using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Data.Models;
using MiNet.Data.Services;
using MiNet.ViewModels.Stories;
using MiNet.Data;

namespace MiNet.Controllers
{
    public class StoriesController : Controller
    {
        private readonly IStoriesService _storiesService;
        
        public StoriesController(IStoriesService storiesService)
        {
            _storiesService = storiesService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(StoryVM storyVM)
        {
            //get the loggedin User Id
            int loggedInUserId = 1;

            var newStory = new Story
            {
                DateCreated = DateTime.Now,
                IsDeleted = false,
                UserId = loggedInUserId
            };

            //Check and save the image
            await _storiesService.CreateStoryAsync(newStory, storyVM.Image);

            return RedirectToAction("Index","Home");
        }
    }
}
