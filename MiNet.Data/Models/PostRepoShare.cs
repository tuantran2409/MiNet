namespace MiNet.Data.Models
{
    /// <summary>
    /// GitHub repo attached to a post (1-1 with Post). Values are a snapshot taken
    /// at share time so the feed does not need to call the GitHub API.
    /// </summary>
    public class PostRepoShare
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public string RepoFullName { get; set; } = string.Empty;
        public string RepoHtmlUrl { get; set; } = string.Empty;
        public string? OwnerAvatarUrl { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public DateTime DateCreated { get; set; }

        // Navigation property
        public Post Post { get; set; } = null!;
    }
}
