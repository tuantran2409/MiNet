using MiNet.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public interface IStoriesService
    {
        Task<List<Story>> GetAllStoriesAsync();
        Task<Story> CreateStoryAsync(Story story);
        Task<bool> RecordStoryViewAsync(int storyId, int viewerId);
        Task<List<User>> GetStoryViewersAsync(int storyId);
        Task<bool> HasUserViewedStoryAsync(int storyId, int userId);
    }
}
