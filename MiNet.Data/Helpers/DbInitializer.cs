using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiNet.Data.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedUsersAndRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // 1. Roles
            foreach (var roleName in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // 2. Admin User
            var adminEmail = "admin@gmail.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new User
                {
                    UserName = "admin.admin",
                    Email = adminEmail,
                    Name = "Admin System",
                    ProfilePictureUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=400&auto=format&fit=crop&q=80",
                    CoverImageUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?w=1200&auto=format&fit=crop&q=80",
                    Bio = "MiNet Platform Administrator & Security Lead 🚀",
                    InterestsJson = JsonSerializer.Serialize(new[] { "#tech", "#ai", "#security", "#management" }),
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminRes = await userManager.CreateAsync(admin, "Coding@1234?");
                if (createAdminRes.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                }
            }

            // 3. 12+ Diverse Industry Users
            var sampleUsers = new List<(string Name, string UserName, string Email, string Bio, string Avatar, string Cover, string[] Interests)>
            {
                ("Tuan Tran", "tuan.tran", "user@gmail.com", "Full-stack Software Engineer & AI enthusiast 💻",
                    "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#tech", "#dotnet", "#ai", "#coding" }),

                ("Sarah Jenkins", "sarah.travel", "sarah.travel@gmail.com", "Solo traveler, nature lover & mountain seeker 🏔️",
                    "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#travel", "#wanderlust", "#nature", "#adventure" }),

                ("Chef Marcus", "marcus.culinary", "marcus.food@gmail.com", "Artisan culinary artist & passionate food storyteller 🍳",
                    "https://images.unsplash.com/photo-1583394838336-acd977736f90?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#food", "#yummy", "#delicious", "#cooking" }),

                ("David Chen", "david.photo", "david.photo@gmail.com", "Chasing street lights and moody cinematic tones 📸",
                    "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#photography", "#art", "#portrait", "#visuals" }),

                ("Emma Watson", "emma.fitness", "emma.fit@gmail.com", "Certified fitness coach & wellness advocate 💪",
                    "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#fitness", "#gym", "#workout", "#health" }),

                ("Lucas Vance", "lucas.audio", "lucas.music@gmail.com", "Electronic music producer & sound designer 🎧",
                    "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#music", "#beats", "#concert", "#audio" }),

                ("Chloe Laurent", "chloe.style", "chloe.fashion@gmail.com", "Minimalist aesthetic, timeless fashion & lifestyle 👗",
                    "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#fashion", "#style", "#beauty", "#ootd" }),

                ("Mark Sterling", "mark.games", "mark.gaming@gmail.com", "Next-gen gaming, tech reviews & speedrunning 🎮",
                    "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#gaming", "#esports", "#stream", "#gamer" }),

                ("Elena Rostova", "elena.design", "elena.design@gmail.com", "Product Designer crafting seamless digital experiences ✨",
                    "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1558655146-d09347e92766?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#design", "#uiux", "#creative", "#minimal" }),

                ("Oliver Brown", "oliver.roast", "oliver.coffee@gmail.com", "Specialty coffee roaster & specialty cafe explorer ☕",
                    "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1442512595331-e89e73853f31?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#coffee", "#cafe", "#latteart", "#morning" }),

                ("Dr. Ryan Miller", "ryan.cosmos", "ryan.science@gmail.com", "Astrophysics, orbital mechanics & deep space wonders 🌌",
                    "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#science", "#space", "#astronomy", "#physics" }),

                ("Maya Lin", "maya.reads", "maya.books@gmail.com", "Books, cozy corners and philosophical reflections 📚",
                    "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=400&auto=format&fit=crop&q=80",
                    "https://images.unsplash.com/photo-1497633762265-9d179a990aa6?w=1200&auto=format&fit=crop&q=80",
                    new[] { "#books", "#reading", "#literature", "#novel" })
            };

            foreach (var su in sampleUsers)
            {
                var userExists = await userManager.FindByEmailAsync(su.Email);
                if (userExists == null)
                {
                    var user = new User
                    {
                        UserName = su.UserName,
                        Email = su.Email,
                        Name = su.Name,
                        Bio = su.Bio,
                        ProfilePictureUrl = su.Avatar,
                        CoverImageUrl = su.Cover,
                        InterestsJson = JsonSerializer.Serialize(su.Interests),
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var res = await userManager.CreateAsync(user, "Coding@1234?");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, AppRoles.User);
                    }
                }
            }
        }

        public static async Task SeedAsync(AppDbContext appDbContext)
        {
            // Wait until users are present
            var users = await appDbContext.Users.ToListAsync();
            if (!users.Any()) return;

            var userDict = users.ToDictionary(u => u.Email ?? u.UserName ?? u.Name, u => u);

            // 1. Seed Posts if few exist
            if (await appDbContext.Posts.CountAsync() < 10)
            {
                var postSamples = new List<(string UserEmail, string Content, string ImageUrl, int DaysAgo)>
                {
                    ("user@gmail.com", "Architecting scalable systems with .NET 9 and ML.NET! Matrix Factorization recommendations running fast and smooth. #tech #dotnet #ai #coding",
                        "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=1080&auto=format&fit=crop&q=80", 1),

                    ("sarah.travel@gmail.com", "Woke up above the clouds in the Swiss Alps today. Nothing beats crisp mountain air and endless views! 🏔️✨ #travel #nature #wanderlust #adventure",
                        "https://images.unsplash.com/photo-1506744038136-46273834b3fb?w=1080&auto=format&fit=crop&q=80", 2),

                    ("marcus.food@gmail.com", "Fresh homemade sourdough straight from the stone oven. That golden crust crackle is pure music! 🍞🔥 #food #cooking #delicious #yummy",
                        "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=1080&auto=format&fit=crop&q=80", 1),

                    ("david.photo@gmail.com", "Night strolls through Tokyo neon corridors. Captured on 35mm f/1.4. #photography #art #portrait #visuals #tokyo",
                        "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?w=1080&auto=format&fit=crop&q=80", 3),

                    ("emma.fit@gmail.com", "Consistency beats motivation every single time. 6 AM kettlebell session locked in! 💪🔥 #fitness #gym #workout #health",
                        "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=1080&auto=format&fit=crop&q=80", 1),

                    ("chloe.fashion@gmail.com", "Autumn trench coats and minimalist neutral palettes. Simplicity is the ultimate sophistication. 🍂 #fashion #style #ootd #beauty",
                        "https://images.unsplash.com/photo-1485968579580-b6d095142e6e?w=1080&auto=format&fit=crop&q=80", 2),

                    ("mark.gaming@gmail.com", "Dual-monitor battle station overhaul complete! Ambient backlight sync is incredible. 🎮⚡ #gaming #esports #stream #tech",
                        "https://images.unsplash.com/photo-1542751371-adc38448a05e?w=1080&auto=format&fit=crop&q=80", 4),

                    ("elena.design@gmail.com", "Design isn't just how it looks, but how effortlessly it works. Crafting clean micro-interactions today. ✨ #design #uiux #creative #minimal",
                        "https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?w=1080&auto=format&fit=crop&q=80", 1),

                    ("oliver.coffee@gmail.com", "Ethiopian Yirgacheffe pour-over: floral notes of jasmine and bergamot. Best way to kick off Saturday! ☕ #coffee #cafe #morning #latteart",
                        "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=1080&auto=format&fit=crop&q=80", 2),

                    ("ryan.science@gmail.com", "James Webb Space Telescope captures mesmerizing gravitational lensing across deep cosmic clusters! 🔭🌌 #science #space #astronomy #physics",
                        "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=1080&auto=format&fit=crop&q=80", 3),

                    ("maya.books@gmail.com", "Revisiting classic Dostoevsky by the fireplace on a rainy afternoon. What are you reading this weekend? 📖 #books #reading #literature #novel",
                        "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=1080&auto=format&fit=crop&q=80", 1),

                    ("leo.music@gmail.com", "Analog synthesizer jamming in the studio. Warm vintage oscillators hit differently! 🎹🎵 #music #beats #audio #concert",
                        "https://images.unsplash.com/photo-1598488035139-bdbb2231ce04?w=1080&auto=format&fit=crop&q=80", 2),

                    ("user@gmail.com", "Exploring real-time collaborative features with ASP.NET Core SignalR WebSockets. Lightning fast delivery! #tech #dotnet #ai",
                        "https://images.unsplash.com/photo-1498050108023-c5249f4df085?w=1080&auto=format&fit=crop&q=80", 5),

                    ("sarah.travel@gmail.com", "Golden hour over the Mediterranean coastline. Turquoise waves and ancient ruins. 🌅🌊 #travel #wanderlust #nature",
                        "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=1080&auto=format&fit=crop&q=80", 4),

                    ("marcus.food@gmail.com", "Truffle tagliolini with aged parmigiano-reggiano and freshly shaved black truffles. Buon appetito! 🍝 #food #delicious #yummy #cooking",
                        "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=1080&auto=format&fit=crop&q=80", 3)
                };

                foreach (var ps in postSamples)
                {
                    if (userDict.TryGetValue(ps.UserEmail, out var author))
                    {
                        var post = new Post
                        {
                            UserId = author.Id,
                            Content = ps.Content,
                            ImageUrl = ps.ImageUrl,
                            DateCreated = DateTime.UtcNow.AddDays(-ps.DaysAgo),
                            DateUpdate = DateTime.UtcNow.AddDays(-ps.DaysAgo),
                            NrOfReports = 0,
                            IsPrivate = false,
                            IsDeleted = false
                        };
                        await appDbContext.Posts.AddAsync(post);
                    }
                }
                await appDbContext.SaveChangesAsync();
            }

            // 2. Seed Hashtags count from posts
            var allPosts = await appDbContext.Posts.ToListAsync();
            foreach (var post in allPosts)
            {
                var tags = HashtagHelper.GetHashtags(post.Content);
                foreach (var t in tags)
                {
                    var existingTag = await appDbContext.Hashtags.FirstOrDefaultAsync(h => h.Name.ToLower() == t.ToLower());
                    if (existingTag == null)
                    {
                        await appDbContext.Hashtags.AddAsync(new Hashtag
                        {
                            Name = t,
                            Count = 1,
                            DateCreated = DateTime.UtcNow,
                            DateUpdate = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existingTag.Count += 1;
                        existingTag.DateUpdate = DateTime.UtcNow;
                    }
                }
            }
            await appDbContext.SaveChangesAsync();

            // 3. Seed Friendships between first 5 users
            if (await appDbContext.Friendships.CountAsync() < 5 && users.Count >= 4)
            {
                var u0 = users[0].Id;
                var u1 = users[1].Id;
                var u2 = users[2].Id;
                var u3 = users[3].Id;

                var sampleFriendships = new List<(int, int)>
                {
                    (u0, u1),
                    (u0, u2),
                    (u1, u3),
                    (u2, u3)
                };

                foreach (var (s, r) in sampleFriendships)
                {
                    bool exists = await appDbContext.Friendships.AnyAsync(f => (f.SenderId == s && f.ReceiverId == r) || (f.SenderId == r && f.ReceiverId == s));
                    if (!exists)
                    {
                        await appDbContext.Friendships.AddAsync(new Friendship
                        {
                            SenderId = s,
                            ReceiverId = r,
                            DateCreated = DateTime.UtcNow.AddDays(-10)
                        });
                    }
                }
                await appDbContext.SaveChangesAsync();
            }

            // 4. Seed UserInteractions for ML.NET Training
            if (await appDbContext.UserInteractions.CountAsync() < 50 && allPosts.Any())
            {
                var random = new Random(42);
                var interactions = new List<UserInteraction>();

                foreach (var user in users)
                {
                    // Select 6-10 random posts each user interacted with
                    var interactedPosts = allPosts.OrderBy(x => random.Next()).Take(8).ToList();
                    foreach (var p in interactedPosts)
                    {
                        // Check if post tags overlap with user interests
                        bool tagOverlap = false;
                        if (!string.IsNullOrEmpty(user.InterestsJson))
                        {
                            var userTags = JsonSerializer.Deserialize<List<string>>(user.InterestsJson) ?? new List<string>();
                            tagOverlap = userTags.Any(ut => p.Content.ToLower().Contains(ut.ToLower()));
                        }

                        // High affinity if tag overlaps
                        float weight = tagOverlap ? (random.Next(5, 8)) : (random.Next(1, 5));
                        var type = weight >= 6 ? InteractionType.Save :
                                   weight >= 4 ? InteractionType.Comment :
                                   weight >= 2 ? InteractionType.Like : InteractionType.View;

                        interactions.Add(new UserInteraction
                        {
                            UserId = user.Id,
                            PostId = p.Id,
                            InteractionType = type,
                            WeightScore = weight,
                            DwellTimeSeconds = random.Next(3, 30),
                            CreatedAt = DateTime.UtcNow.AddHours(-random.Next(1, 100))
                        });

                        // Also add Likes if Like/Comment/Save
                        if (weight >= 3 && !await appDbContext.Likes.AnyAsync(l => l.UserId == user.Id && l.PostId == p.Id))
                        {
                            await appDbContext.Likes.AddAsync(new Like { UserId = user.Id, PostId = p.Id });
                        }
                    }
                }

                await appDbContext.UserInteractions.AddRangeAsync(interactions);
                await appDbContext.SaveChangesAsync();
            }
        }
    }
}
