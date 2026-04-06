using Microsoft.EntityFrameworkCore;
using MiNet.Data.Helpers.Enums;  
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNet.Data.Services
{
    public class UsersService : IUsersService
    {
        private readonly AppDbContext _appDbContext;
        public UsersService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<User> GetUser(int loggedInUserId)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserId) ?? new User();
        }

        public async Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl)
        {
            var userDb = await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserId);

            if (userDb != null)
            {
                userDb.ProfilePictureUrl = profilePictureUrl;
                _appDbContext.Users.Update(userDb);
                await _appDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> GetUserPosts(int userId)
        {
            var allPosts = await _appDbContext.Posts
                .Where(n => n.UserId == userId && n.Reports.Count < 5 && !n.IsDeleted)
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(n => n.User)
                .Include(n => n.Reports)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return allPosts;
        }

       
        public async Task<List<User>> GetFriendsAsync(int userId)
        {
            
            var friendships = await _appDbContext.Friendships
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == FriendshipStatus.Accepted)
                .ToListAsync();

            var friendIds = friendships.Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId).ToList();

            return await _appDbContext.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToListAsync();
        }
    }
}