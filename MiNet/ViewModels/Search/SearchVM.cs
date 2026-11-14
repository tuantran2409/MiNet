using MiNet.Data.Models;
using System.Collections.Generic;

namespace MiNet.ViewModels.Search
{
    public class SearchVM
    {
        public string Query { get; set; } = string.Empty;
        public string ActiveTab { get; set; } = "all"; // all, users, posts, tags
        public List<User> Users { get; set; } = new List<User>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
    }
}
