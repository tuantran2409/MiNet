namespace MiNet.Data.Models
{
    /// <summary>
    /// A GitHub repo saved by a user from the Explore page. One row per (user, repo).
    /// </summary>
    public class RepoBookmark
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string RepoFullName { get; set; } = string.Empty;
        public string RepoHtmlUrl { get; set; } = string.Empty;
        public string? OwnerAvatarUrl { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public int Stars { get; set; }
        public DateTime DateCreated { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}
