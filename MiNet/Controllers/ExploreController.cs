using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNet.Controllers.Base;
using MiNet.Data.Dtos;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using MiNet.ViewModels.Explore;
using System.Text.RegularExpressions;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class ExploreController : BaseController
    {
        private const int DefaultRangeInDays = 7;
        private const int MaxRangeInDays = 366;

        // GitHub opened in Oct 2007; block earlier dates so the query is meaningful
        private static readonly DateTime EarliestDate = new DateTime(2007, 10, 1);

        private static readonly Regex RepoNamePattern =
            new Regex(@"^[A-Za-z0-9._-]+/[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        private readonly IGitHubService _gitHubService;
        private readonly IPostsService _postsService;
        private readonly IRepoBookmarksService _repoBookmarksService;

        public ExploreController(IGitHubService gitHubService,
            IPostsService postsService,
            IRepoBookmarksService repoBookmarksService)
        {
            _gitHubService = gitHubService;
            _postsService = postsService;
            _repoBookmarksService = repoBookmarksService;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? language)
        {
            var (rangeFrom, rangeTo) = NormalizeRange(from, to);
            var selectedLanguage = string.IsNullOrWhiteSpace(language) ? null : language.Trim();

            var result = await _gitHubService.SearchTopReposAsync(rangeFrom, rangeTo, selectedLanguage);

            // Repos saved by the current user, so each card can render Save/Saved correctly
            var savedRepos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var loggedInUserId = GetUserId();
            if (loggedInUserId.HasValue)
            {
                var bookmarks = await _repoBookmarksService.GetUserBookmarksAsync(loggedInUserId.Value);
                savedRepos = new HashSet<string>(bookmarks.Select(b => b.RepoFullName), StringComparer.OrdinalIgnoreCase);
            }

            var viewModel = new ExploreVM
            {
                From = rangeFrom,
                To = rangeTo,
                Language = selectedLanguage,
                Items = result.Items,
                TotalCount = result.TotalCount,
                RateLimited = result.RateLimited,
                FromCache = result.FromCache,
                Error = result.Error,
                RemainingRequests = result.RemainingRequests,
                RateLimitResetAt = result.RateLimitResetAt,
                SavedRepos = savedRepos
            };

            return View(viewModel);
        }

        // ==================== Share / Save ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareRepo(ShareRepoVM shareRepoVM)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();

            if (!TryBuildRepo(shareRepoVM.RepoFullName, shareRepoVM.RepoHtmlUrl, shareRepoVM.OwnerAvatarUrl,
                    shareRepoVM.Description, shareRepoVM.Language, shareRepoVM.Stars, shareRepoVM.Forks, out var repo))
            {
                TempData["ExploreError"] = "That repository data is not valid.";
                return RedirectToAction("Index");
            }

            // Stop the same user from flooding the feed with the same repo
            if (await _postsService.HasUserSharedRepoSinceAsync(userId.Value, repo.FullName, DateTime.Now.AddHours(-1)))
            {
                TempData["ExploreError"] = $"You already shared {repo.FullName} in the last hour.";
                return RedirectToAction("Index");
            }

            await _postsService.CreateRepoSharePostAsync(userId.Value, repo, shareRepoVM.Comment);
            TempData["FeedOk"] = $"Shared {repo.FullName} to your feed.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBookmark(BookmarkRepoVM bookmarkRepoVM)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();

            if (!TryBuildRepo(bookmarkRepoVM.RepoFullName, bookmarkRepoVM.RepoHtmlUrl, bookmarkRepoVM.OwnerAvatarUrl,
                    bookmarkRepoVM.Description, bookmarkRepoVM.Language, bookmarkRepoVM.Stars, 0, out var repo))
            {
                TempData["ExploreError"] = "That repository data is not valid.";
                return RedirectToAction("Index");
            }

            var isSaved = await _repoBookmarksService.ToggleAsync(userId.Value, repo);
            TempData["ExploreOk"] = isSaved ? $"Saved {repo.FullName}." : $"Removed {repo.FullName} from saved.";

            return RedirectToLocal(bookmarkRepoVM.ReturnUrl) ?? RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Bookmarks()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();

            var bookmarks = await _repoBookmarksService.GetUserBookmarksAsync(userId.Value);
            return View(bookmarks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveBookmark(string repoFullName, string? returnUrl)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();

            if (!string.IsNullOrWhiteSpace(repoFullName))
                await _repoBookmarksService.RemoveAsync(userId.Value, repoFullName);

            TempData["ExploreOk"] = $"Removed {repoFullName} from saved.";
            return RedirectToLocal(returnUrl) ?? RedirectToAction("Bookmarks");
        }

        /// <summary>
        /// Redirects only to local URLs, so a crafted ReturnUrl cannot bounce the user off-site.
        /// </summary>
        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return null;
        }

        /// <summary>
        /// Validates the repo values posted by an Explore card and maps them to a DTO.
        /// Only https://github.com/... links with an owner/name pair are accepted.
        /// </summary>
        private static bool TryBuildRepo(string? fullName, string? htmlUrl, string? avatarUrl, string? description,
            string? language, int stars, int forks, out GitHubRepoDto repo)
        {
            repo = new GitHubRepoDto();

            if (string.IsNullOrWhiteSpace(fullName) || !RepoNamePattern.IsMatch(fullName.Trim()))
                return false;

            if (string.IsNullOrWhiteSpace(htmlUrl) || !Uri.TryCreate(htmlUrl, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttps || !uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                return false;

            repo = new GitHubRepoDto
            {
                FullName = fullName.Trim(),
                HtmlUrl = uri.ToString(),
                OwnerAvatarUrl = avatarUrl,
                Description = description,
                Language = language,
                Stars = Math.Max(0, stars),
                Forks = Math.Max(0, forks)
            };

            return true;
        }

        /// <summary>
        /// Normalizes the range picked by the user: defaults to the last 7 days,
        /// never in the future, never before 2007-10-01, at most 366 days, swaps from/to when reversed.
        /// </summary>
        private static (DateTime From, DateTime To) NormalizeRange(DateTime? from, DateTime? to)
        {
            var today = DateTime.Today;
            var rangeTo = (to ?? today).Date;
            var rangeFrom = (from ?? rangeTo.AddDays(-(DefaultRangeInDays - 1))).Date;

            if (rangeTo > today) rangeTo = today;
            if (rangeTo < EarliestDate) rangeTo = EarliestDate;
            if (rangeFrom > rangeTo) (rangeFrom, rangeTo) = (rangeTo, rangeFrom);
            if (rangeFrom < EarliestDate) rangeFrom = EarliestDate;
            if ((rangeTo - rangeFrom).TotalDays >= MaxRangeInDays) rangeFrom = rangeTo.AddDays(-(MaxRangeInDays - 1));

            return (rangeFrom, rangeTo);
        }
    }
}
