using MiNet.Data.Models;

namespace MiNet.ViewModels.Users
{
    public class GetUserProfileVM
    {
        public User User { get; set; }
        public List<Post> Posts { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public List<User> Friends { get; set; } = new List<User>();
    }
}
