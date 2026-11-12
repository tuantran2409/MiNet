using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using MiNet.Data.Dtos;

namespace MiNet.Data.Services
{
    /// <summary>
    /// Calls the GitHub Search API: GET /search/repositories?q=created:A..B&sort=stars&order=desc
    /// Rate limit: 10 requests/minute without a token, 30 requests/minute with one,
    /// so every successful result is cached in memory.
    /// </summary>
    public class GitHubService : IGitHubService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public GitHubService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<GitHubRepoSearchResult> SearchTopReposAsync(DateTime from, DateTime to, string? language, int perPage = 25)
        {
            perPage = Math.Clamp(perPage, 1, 100);

            var languageKey = string.IsNullOrWhiteSpace(language) ? "all" : language.Trim().ToLowerInvariant();
            var cacheKey = $"github-top:{from:yyyyMMdd}:{to:yyyyMMdd}:{languageKey}:{perPage}";

            if (_cache.TryGetValue(cacheKey, out GitHubRepoSearchResult? cached) && cached != null)
            {
                // Return a copy so the view cannot mutate the cached instance
                return new GitHubRepoSearchResult
                {
                    Items = cached.Items,
                    TotalCount = cached.TotalCount,
                    RateLimited = cached.RateLimited,
                    Error = cached.Error,
                    RemainingRequests = cached.RemainingRequests,
                    RateLimitResetAt = cached.RateLimitResetAt,
                    FromCache = true
                };
            }

            var query = $"created:{from:yyyy-MM-dd}..{to:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(language))
                query += $" language:\"{language.Trim()}\"";

            var requestUrl = $"search/repositories?q={Uri.EscapeDataString(query)}&sort=stars&order=desc&per_page={perPage}&page=1";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(requestUrl);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                return new GitHubRepoSearchResult
                {
                    Error = "Could not reach GitHub. Check your network connection and try again."
                };
            }

            using (response)
            {
                var result = new GitHubRepoSearchResult
                {
                    RemainingRequests = ReadRemainingQuota(response),
                    RateLimitResetAt = ReadRateLimitReset(response)
                };

                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    result.RateLimited = true;
                    result.Error = "GitHub blocked the request: search rate limit reached.";
                    return result;
                }

                if (!response.IsSuccessStatusCode)
                {
                    result.Error = $"GitHub returned error {(int)response.StatusCode}.";
                    return result;
                }

                GitHubSearchResponse? payload;
                try
                {
                    payload = await response.Content.ReadFromJsonAsync<GitHubSearchResponse>(JsonOptions);
                }
                catch (JsonException)
                {
                    result.Error = "Could not read the response returned by GitHub.";
                    return result;
                }

                if (payload == null)
                {
                    result.Error = "GitHub returned an empty response.";
                    return result;
                }

                result.TotalCount = payload.TotalCount;
                result.Items = payload.Items.Select(MapToDto).ToList();

                _cache.Set(cacheKey, result, CacheDuration);
                return result;
            }
        }

        private static GitHubRepoDto MapToDto(GitHubSearchItem item)
        {
            return new GitHubRepoDto
            {
                FullName = item.FullName ?? string.Empty,
                HtmlUrl = item.HtmlUrl ?? string.Empty,
                Description = item.Description,
                Language = item.Language,
                Stars = item.StargazersCount,
                Forks = item.ForksCount,
                OpenIssues = item.OpenIssuesCount,
                CreatedAt = item.CreatedAt,
                PushedAt = item.PushedAt,
                OwnerLogin = item.Owner?.Login,
                OwnerAvatarUrl = item.Owner?.AvatarUrl,
                Topics = item.Topics ?? new List<string>()
            };
        }

        private static int? ReadRemainingQuota(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var values))
            {
                var raw = values.FirstOrDefault();
                if (int.TryParse(raw, out var remaining))
                    return remaining;
            }
            return null;
        }

        private static DateTimeOffset? ReadRateLimitReset(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("X-RateLimit-Reset", out var values))
            {
                var raw = values.FirstOrDefault();
                if (long.TryParse(raw, out var unixSeconds))
                    return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToLocalTime();
            }
            return null;
        }
    }
}
