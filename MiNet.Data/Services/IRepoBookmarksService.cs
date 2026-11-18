using MiNet.Data.Dtos;
using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public interface IRepoBookmarksService
    {
        /// <summary>
        /// Adds the repo to the user's saved list, or removes it when it is already saved.
        /// Returns true when the repo is saved after the call.
        /// </summary>
        Task<bool> ToggleAsync(int userId, GitHubRepoDto repo);

        Task<bool> IsBookmarkedAsync(int userId, string repoFullName);

        Task<List<RepoBookmark>> GetUserBookmarksAsync(int userId);

        Task<bool> RemoveAsync(int userId, string repoFullName);
    }
}
