using EDFRR.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EDFRR.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper()
                });
            }
        }

        await SeedUserAsync(userManager, "admin@edfrr.com", "Admin@123", "Admin", "User", "Admin");
        await SeedUserAsync(userManager, "user@edfrr.com", "User@123", "Test", "User", "User");
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NormalizedUserName = email.ToUpper(),
                NormalizedEmail = email.ToUpper(),
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
            return;
        }

        bool needsUpdate = false;

        if (user.Email != email)
        {
            user.Email = email;
            needsUpdate = true;
        }
        if (user.NormalizedEmail != email.ToUpper())
        {
            user.NormalizedEmail = email.ToUpper();
            needsUpdate = true;
        }
        if (user.UserName != email)
        {
            user.UserName = email;
            needsUpdate = true;
        }
        if (user.NormalizedUserName != email.ToUpper())
        {
            user.NormalizedUserName = email.ToUpper();
            needsUpdate = true;
        }
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            await userManager.UpdateAsync(user);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await userManager.ResetPasswordAsync(user, token, password);

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
