namespace MiNet.ViewModels.Settings
{
    public class UpdateProfileVM
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Interests { get; set; }
    }
}
