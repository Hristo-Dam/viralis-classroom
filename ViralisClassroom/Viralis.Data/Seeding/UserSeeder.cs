using Microsoft.AspNetCore.Identity;
using Viralis.Data.Constants;
using Viralis.Data.Models;

namespace Viralis.Data.Seeding
{
    public class UserSeeder
    {
        public static IEnumerable<ApplicationUser> SeedUsers()
        {
            List<ApplicationUser> users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    Id = UserConstants.Id,
                    UserName = UserConstants.UserName,
                    NormalizedUserName = UserConstants.UserName.ToUpper(),
                    Email = UserConstants.Email,
                    NormalizedEmail = UserConstants.Email.ToUpper(),
                    ConcurrencyStamp = UserConstants.ConcurrencyStamp,
                },
                new ApplicationUser()
                {
                    Id = UserConstants.AdminId,
                    UserName = UserConstants.AdminUserName,
                    NormalizedUserName = UserConstants.AdminUserName.ToUpper(),
                    Email = UserConstants.AdminEmail,
                    NormalizedEmail = UserConstants.AdminEmail.ToUpper(),
                    ConcurrencyStamp = UserConstants.AdminConcurrencyStamp,
                }
            };

            var hasher = new PasswordHasher<ApplicationUser>();

            foreach (var user in users)
            {
                user.PasswordHash = hasher.HashPassword(user, UserConstants.DefaultPassword);
            }

            return users;
        }
    }
}
