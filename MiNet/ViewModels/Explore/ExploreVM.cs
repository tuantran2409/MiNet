using MiNet.Data.Models;
using System.Collections.Generic;

namespace MiNet.ViewModels.Explore
{
    public class ExploreVM
    {
        public string? SelectedTag { get; set; }
        public List<Hashtag> TrendingTags { get; set; } = new List<Hashtag>();
        public List<User> PopularCreators { get; set; } = new List<User>();
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}
