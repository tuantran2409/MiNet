using System.Collections.Generic;
using MiNet.Data.Models;

namespace MiNet.ViewModels.Chat
{
    public class ChatListVM
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        public int CurrentUserId { get; set; }
    }

    public class ConversationDetailsVM
    {
        public Conversation Conversation { get; set; } = null!;
        public List<Message> Messages { get; set; } = new List<Message>();
        public int CurrentUserId { get; set; }
    }
}
