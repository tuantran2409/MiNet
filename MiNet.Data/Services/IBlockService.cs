using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public interface IBlockService
    {
        Task<bool> BlockUserAsync(int blockerId, int blockedId);
        Task<bool> UnblockUserAsync(int blockerId, int blockedId);
        Task<bool> IsBlockedAsync(int userA, int userB);
        Task<List<int>> GetBlockedUserIdsAsync(int userId);
    }
}

