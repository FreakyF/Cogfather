using Cogfather.HQ.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Cogfather.HQ.Infrastructure.Data;

public class DbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DbSeeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        if (!await _roleManager.RoleExistsAsync("Admin")) await _roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminEmail = "admin@cogfather.local";
        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded) await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}