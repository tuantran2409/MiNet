using System;
using System.ComponentModel.DataAnnotations;

namespace MiNet.Data.Models
{
    public enum InteractionType
    {
        View = 1,
        Like = 2,
        Comment = 3,
        Save = 4,
        Share = 5
    }

    public class UserInteraction
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public InteractionType InteractionType { get; set; }
        public int DwellTimeSeconds { get; set; }
        public float WeightScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public Post Post { get; set; } = null!;
    }
}
