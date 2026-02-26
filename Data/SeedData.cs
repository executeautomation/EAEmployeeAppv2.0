using EAEmployee.Net8.Models;
using Microsoft.AspNetCore.Identity;

namespace EAEmployee.Net8.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Create roles
        string[] roles = { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed default admin user
        const string adminUser = "admin";
        const string adminEmail = "admin@executeautomation.com";
        const string adminPassword = "password";

        if (await userManager.FindByNameAsync(adminUser) == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminUser,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Administrator");
            }
        }
    }
}
