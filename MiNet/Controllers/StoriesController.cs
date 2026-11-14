using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Helpers.Enums;
using MiNet.Data.Models;
using MiNet.Data.Services;
using MiNet.ViewModels.Stories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class StoriesController : BaseController
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
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var imageUploadPath = await _filesService.UploadImageAsync(storyVM.Image, ImageFileType.StoryImage);

            var newStory = new Story
            {
                DateCreated = DateTime.UtcNow,
                IsDeleted = false,
                ImageUrl = imageUploadPath,
                UserId = loggedInUserId.Value
            };

            await _storiesService.CreateStoryAsync(newStory);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RecordView(int storyId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return Unauthorized();

            bool success = await _storiesService.RecordStoryViewAsync(storyId, loggedInUserId.Value);
            return Json(new { success });
        }

        [HttpGet]
        public async Task<IActionResult> GetViewers(int storyId)
        {
            var viewers = await _storiesService.GetStoryViewersAsync(storyId);
            return Json(viewers.Select(v => new
            {
                id = v.Id,
                name = v.Name,
                avatar = v.ProfilePictureUrl
            }));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHighlight(int storyId, string? title)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return Unauthorized();

            bool isHighlight = await _storiesService.ToggleStoryHighlightAsync(storyId, title);
            return Json(new { success = true, isHighlight });
        }
    }
}
