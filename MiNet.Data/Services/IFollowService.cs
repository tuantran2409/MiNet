using MiNet.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public interface IFollowService
    {
        Task<bool> ToggleFollowAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
        Task<List<User>> GetFollowersAsync(int userId);
        Task<List<User>> GetFollowingAsync(int userId);
    }
}

