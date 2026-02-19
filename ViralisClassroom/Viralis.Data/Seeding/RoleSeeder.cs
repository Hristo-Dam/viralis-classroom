using Microsoft.AspNetCore.Identity;
using Viralis.Common.Constants;

namespace Viralis.Data.Seeding
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] roles = { RoleConstants.ADMIN, RoleConstants.TEACHER, RoleConstants.STUDENT, RoleConstants.SCHOOL_ADMINISTRATOR };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    });
                }
            }
        }
    }
}
