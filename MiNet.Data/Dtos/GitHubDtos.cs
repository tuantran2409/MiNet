using System.Text.Json.Serialization;

namespace MiNet.Data.Dtos
{
    /// <summary>
    /// One GitHub repo mapped from the Search API payload,
    /// keeping only the fields the Explore UI renders.
    /// </summary>
    public class GitHubRepoDto
    {
        public string FullName { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Language { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public int OpenIssues { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PushedAt { get; set; }
        public string? OwnerLogin { get; set; }
        public string? OwnerAvatarUrl { get; set; }
        public List<string> Topics { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result handed back to the controller. RateLimited/Error are mapped from the HTTP status
    /// so the view can render a message instead of throwing.
    /// </summary>
    public class GitHubRepoSearchResult
    {
        public List<GitHubRepoDto> Items { get; set; } = new List<GitHubRepoDto>();
        public int TotalCount { get; set; }
        public bool RateLimited { get; set; }
        public bool FromCache { get; set; }
        public string? Error { get; set; }

        // Rate limit values from the response headers (null when not available)
        public int? RemainingRequests { get; set; }
        public DateTimeOffset? RateLimitResetAt { get; set; }
    }

    // ===== Raw GitHub Search API payload (snake_case) =====

    internal class GitHubSearchResponse
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("incomplete_results")]
        public bool IncompleteResults { get; set; }

        [JsonPropertyName("items")]
        public List<GitHubSearchItem> Items { get; set; } = new List<GitHubSearchItem>();
    }

    internal class GitHubSearchItem
    {
        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }

        [JsonPropertyName("forks_count")]
        public int ForksCount { get; set; }

        [JsonPropertyName("open_issues_count")]
        public int OpenIssuesCount { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("pushed_at")]
        public DateTime PushedAt { get; set; }

        [JsonPropertyName("topics")]
        public List<string> Topics { get; set; } = new List<string>();

        [JsonPropertyName("owner")]
        public GitHubSearchOwner? Owner { get; set; }
    }

    internal class GitHubSearchOwner
    {
        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }
    }
}
