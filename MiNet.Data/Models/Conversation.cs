using System;
using System.Collections.Generic;

namespace MiNet.Data.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public DateTime DateCreated { get; set; }

        // Navigation properties
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    }
}
