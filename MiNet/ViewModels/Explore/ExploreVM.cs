using MiNet.Data.Dtos;

namespace MiNet.ViewModels.Explore
{
    public class ExploreVM
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string? Language { get; set; }
        public List<GitHubRepoDto> Items { get; set; } = new List<GitHubRepoDto>();
        public int TotalCount { get; set; }
        public bool RateLimited { get; set; }
        public bool FromCache { get; set; }
        public string? Error { get; set; }
        public int? RemainingRequests { get; set; }
        public DateTimeOffset? RateLimitResetAt { get; set; }

        // Number of days in the selected range — used to highlight the preset button
        public int RangeInDays => (To.Date - From.Date).Days + 1;

        // Language values for the dropdown — must match the names GitHub uses in `language:`
        public static readonly IReadOnlyList<string> Languages = new List<string>
        {
            "C#", "JavaScript", "TypeScript", "Python", "Java", "Go", "Rust", "C++",
            "C", "PHP", "Ruby", "Swift", "Kotlin", "Dart", "Shell", "HTML", "CSS", "Vue", "Svelte"
        };
    }
}
