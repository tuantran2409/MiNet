using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiNet.Controllers.Base;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using MiNet.ViewModels.Explore;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class ExploreController : BaseController
    {
        private const int DefaultRangeInDays = 7;
        private const int MaxRangeInDays = 366;

        // GitHub opened in Oct 2007; block earlier dates so the query is meaningful
        private static readonly DateTime EarliestDate = new DateTime(2007, 10, 1);

        private readonly IGitHubService _gitHubService;

        public ExploreController(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? language)
        {
            var (rangeFrom, rangeTo) = NormalizeRange(from, to);
            var selectedLanguage = string.IsNullOrWhiteSpace(language) ? null : language.Trim();

            var result = await _gitHubService.SearchTopReposAsync(rangeFrom, rangeTo, selectedLanguage);

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
                RateLimitResetAt = result.RateLimitResetAt
            };

            return View(viewModel);
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
