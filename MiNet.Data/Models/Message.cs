using System;

namespace MiNet.Data.Models
{
    public enum MessageType
    {
        Text,
        Image,
        File
    }

    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int? SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public string? FileUrl { get; set; }
        public DateTime DateSent { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public User? Sender { get; set; }
    }
}
