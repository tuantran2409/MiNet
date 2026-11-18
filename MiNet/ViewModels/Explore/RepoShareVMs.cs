namespace MiNet.ViewModels.Explore
{
    /// <summary>Payload posted when the user shares an Explore card to the feed.</summary>
    public class ShareRepoVM
    {
        public string RepoFullName { get; set; } = string.Empty;
        public string RepoHtmlUrl { get; set; } = string.Empty;
        public string? OwnerAvatarUrl { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public string? Comment { get; set; }
    }

    /// <summary>Payload posted when the user saves/unsaves an Explore card.</summary>
    public class BookmarkRepoVM
    {
        public string RepoFullName { get; set; } = string.Empty;
        public string RepoHtmlUrl { get; set; } = string.Empty;
        public string? OwnerAvatarUrl { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public int Stars { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
