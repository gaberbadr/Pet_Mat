using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace RepositoryLayer.Data.Data_seeding
{
    public static class IdentitySeeder
    {
        public static async Task SeedAppUserAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            // 1. Ensure roles exist
            string[] roles = { "Admin", "Doctor", "Pharmacy", "AdminAssistant" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed user if no users exist
            if (!userManager.Users.Any())
            {
                // create and save new address first
                var newAddress = new Address
                {
                    City = "System",
                    Government = "System",
                    Country = "System" 
                };

                await unitOfWork.Repository<Address, int>().AddAsync(newAddress);
                await unitOfWork.CompleteAsync(); // make sure Id is generated

                var user = new ApplicationUser
                {
                    UserName = "gaberemadbader@gmail.com",
                    PhoneNumber = "01019806684",
                    FirstName = "Gaber",
                    LastName = "Badr",
                    Email = "gaberemadbader@gmail.com",
                    EmailConfirmed = true,
                    AddressId = newAddress.Id,
                    HasPasswordAsync = true
                };

                // 3. Create the user with password
                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    // 4. Assign the Admin role
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    // log errors if user creation failed
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Code} - {error.Description}");
                    }
                }
            }
        }
    }
}
