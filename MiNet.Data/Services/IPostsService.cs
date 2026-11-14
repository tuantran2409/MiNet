using MiNet.Data.Dtos;
using MiNet.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public interface IPostsService
    {
        Task<List<Post>> GetAllPostsAsync(int loggedInUserId);
        Task<List<Post>> GetPersonalizedFeedAsync(int currentUserId, int page = 1, int pageSize = 30);
        Task<Post?> GetPostByIdAsync(int postId);
        Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId);
        Task<Post> CreatePostAsync(Post post);
        Task<Post?> RemovePostAsync(int postId);

        Task AddPostCommentAsync(Comment comment);
        Task RemovePostCommentAsync(int commentId);

        Task<GetNotificationDto> TogglePostLikeAsync(int postId, int userId);
        Task<GetNotificationDto> SetPostReactionAsync(int postId, int userId, ReactionType reactionType);
        Task<GetNotificationDto> TogglePostFavoriteAsync(int postId, int userId);
        Task TogglePostVisibilityAsync(int postId, int userId);
        Task ReportPostAsync(int postId, int userId);

        Task<List<Post>> GetExplorePostsAsync(int count = 30);
    }
}
