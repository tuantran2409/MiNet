using MiNet.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public interface IUserInteractionService
    {
        Task RecordInteractionAsync(int userId, int postId, InteractionType type, int dwellSeconds = 0);
        Task<int> GetUserInteractionsCountAsync(int userId);
        Task<List<string>> GetUserInterestsAsync(int userId);
    }
}
