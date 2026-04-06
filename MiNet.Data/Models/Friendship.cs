using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNet.Data.Helpers.Enums;  

namespace MiNet.Data.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }

        public int SenderId { get; set; }
        public virtual User Sender { get; set; }

        public int ReceiverId { get; set; }
        public virtual User Receiver { get; set; }

       
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    }
}

