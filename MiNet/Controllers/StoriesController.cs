using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiNet.Data;
using MiNet.Data.Helpers.Enums;
using MiNet.Data.Models;
using MiNet.Data.Services;
using MiNet.ViewModels.Stories;


namespace MiNet.Controllers
{
    public class StoriesController : Controller
    {
        private readonly IStoriesService _storiesService;
        private readonly IFilesService _filesService;
        
        public StoriesController(IStoriesService storiesService, IFilesService filesService)
        {
            _storiesService = storiesService;
            _filesService = filesService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(StoryVM storyVM)
        {
            //get the loggedin User Id
            int loggedInUserId = 1;

            //Check and save the img url
            var imageUploadPath = await _filesService.UploadImageAsync(storyVM.Image, ImageFileType.StoryImage);

            var newStory = new Story
            {
                DateCreated = DateTime.Now,
                IsDeleted = false,
                ImageUrl = imageUploadPath,
                UserId = loggedInUserId
            };


            //Add story to the database
            await _storiesService.CreateStoryAsync(newStory);

            return RedirectToAction("Index","Home");
        }
    }
}
