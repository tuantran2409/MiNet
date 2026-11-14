using System;

namespace MiNet.Data.Models
{
    public class StoryView
    {
        public int StoryId { get; set; }
        public Story Story { get; set; } = null!;

        public int ViewerId { get; set; }
        public User Viewer { get; set; } = null!;

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
