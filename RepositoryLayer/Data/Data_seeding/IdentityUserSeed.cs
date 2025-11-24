using System;
using System.Linq;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace RepositoryLayer.Data.Data_seeding
{
    public static class IdentitySeeder
    {
        public static async Task SeedAppUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Create roles
            string[] roles = { "Admin", "Doctor", "Pharmacy", "AdminAssistant" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Create Admin user if no users exist
            if (!userManager.Users.Any())
            {
                var user = new ApplicationUser
                {
                    UserName = "gaberemadbader@gmail.com",
                    PhoneNumber = "01019806684",
                    FirstName = "Gaber",
                    LastName = "Badr",
                    Email = "gaberemadbader@gmail.com",
                    EmailConfirmed = true,
                    HasPasswordAsync = true,

                    // Address added INSIDE Identity context
                    Address = new Address
                    {
                        City = "System",
                        Government = "System"
                    }
                };

                var createResult = await userManager.CreateAsync(user, "Admin@123");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        Console.WriteLine($"Error: {error.Code} - {error.Description}");
                    }
                }
            }
        }
    }
}
