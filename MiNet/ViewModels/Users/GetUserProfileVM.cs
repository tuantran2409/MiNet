using MiNet.Data.Models;
using System.Collections.Generic;

namespace MiNet.ViewModels.Users
{
    public class GetUserProfileVM
    {
        public User User { get; set; } = null!;
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Post> SavedPosts { get; set; } = new List<Post>();
        public IList<string> Roles { get; set; } = new List<string>();
        public List<User> Friends { get; set; } = new List<User>();

        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
        public bool IsFriend { get; set; }
        public bool HasPendingFriendRequest { get; set; }
        public bool IsSelf { get; set; }
    }
}
