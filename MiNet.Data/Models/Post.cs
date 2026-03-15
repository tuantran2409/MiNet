using System.ComponentModel.DataAnnotations;

namespace MiNet.Data.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public String Content { get; set; }
        public String? ImageUrl { get; set; }
        public int NrOfReports { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdate { get; set; }

        //Foreign key
        public int UserId { get; set; }

        //Navigation properties
        public User User { get; set; }
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
