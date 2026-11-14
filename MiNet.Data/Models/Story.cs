using System;
using System.Collections.Generic;

namespace MiNet.Data.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDeleted { get; set; }

        // Story Highlights
        public bool IsHighlight { get; set; } = false;
        public string? HighlightTitle { get; set; }

        // Foreign key
        public int UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
    }
}
