namespace MiNet.ViewModels.Home
{
    /// <summary>Payload posted by the "Edit Post" modal.</summary>
    public class PostEditVM
    {
        public int PostId { get; set; }
        public string? Content { get; set; }
    }
}
