using MiNet.Data.Models;

namespace MiNet.ViewModels.Home
{
    public class PostReactionVM
    {
        public int PostId { get; set; }
        public ReactionType ReactionType { get; set; } = ReactionType.Like;
    }
}
