using System.ComponentModel.DataAnnotations;

namespace MiNet.ViewModels.Home
{
    public class PostCommentVM
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận")]
        public string Content { get; set; } = string.Empty; 
    }
}