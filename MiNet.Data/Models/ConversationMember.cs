using System;

namespace MiNet.Data.Models
{
    public class ConversationMember
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedDate { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
