using MiNet.Data.Dtos;

namespace MiNet.Data.Services
{
    public interface IGitHubService
    {
        /// <summary>
        /// Returns repos created within [from, to] (inclusive), ordered by total stars descending.
        /// Results are cached in memory to avoid burning the GitHub Search API rate limit.
        /// </summary>
        /// <param name="language">GitHub language name (e.g. "C#", "TypeScript"); null/empty = all languages.</param>
        Task<GitHubRepoSearchResult> SearchTopReposAsync(DateTime from, DateTime to, string? language, int perPage = 25);
    }
}
