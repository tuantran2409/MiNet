using System;

namespace MiNet.Data.Models
{
    public enum ReactionType
    {
        Like = 1,
        Love = 2,
        Haha = 3,
        Wow = 4,
        Sad = 5,
        Angry = 6
    }

    public class Like
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }

        public ReactionType ReactionType { get; set; } = ReactionType.Like;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Post Post { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
