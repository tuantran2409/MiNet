using MiNet.Data.Models;
using MiNet.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiNet.Data.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext appDbContext) 
        {
            //if(!appDbContext.Users.Any() && !appDbContext.Posts.Any())
            //{
            //    var newUser = new User()
            //    {
            //        Name = "Tran Hung Anh Tuan",
            //        ProfilePictureUrl = "https://yt3.ggpht.com/wYT1U9NoL8gttISEdKuIA9cVAWlz9Rm2CbEqVPmYbtzUU0twh6KAL_e5jyUvK4nTiQSFO1tGMw=s600-c-k-c0x00ffffff-no-rj-rp-mo"
            //    };
            //    await appDbContext.AddAsync(newUser);
            //    await appDbContext.SaveChangesAsync();
            //    var newPostWithoutImg = new Post()
            //    {
            //        Content = "This is going to be the first post being loaded from database and created by the default user contain text only.",
            //        ImageUrl = "",
            //        NrOfReports = 0,
            //        DateCreated = DateTime.Now,
            //        DateUpdate = DateTime.Now,
            //        UserId = newUser.Id,
            //    };

            //    var newPostWithImg = new Post()
            //    {
            //        Content = "This is going to be the second post being loaded from database and created by the default user contain text and image.",
            //        ImageUrl = "https://plus.unsplash.com/premium_photo-1661963063875-7f131e02bf75?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", 
            //        NrOfReports = 0,
            //        DateCreated = DateTime.Now,
            //        DateUpdate = DateTime.Now,
            //        UserId = newUser.Id,
            //    };

            //    await appDbContext.Posts.AddRangeAsync(newPostWithoutImg, newPostWithImg);
            //    await appDbContext.SaveChangesAsync();
            //}
        }
    }
}
