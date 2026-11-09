using Microsoft.AspNetCore.Identity;
using WatchStore.Web.Entities;

namespace WatchStore.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        foreach (var roleName in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
        if (await userManager.FindByNameAsync("admin") is not null)
            return;

        var admin = new User
        {
            UserName = "admin",
            IsActive = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@123");

        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
