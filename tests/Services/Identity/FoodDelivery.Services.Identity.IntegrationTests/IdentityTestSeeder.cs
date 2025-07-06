using System.Globalization;
using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Shared;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.IntegrationTests;

public class IdentityTestSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : ITestDataSeeder
{
    public int Order => 1;

    public async Task SeedAllAsync(CancellationToken cancellationToken)
    {
        await SeedRoles();
        await SeedUsers();
    }

    private async Task SeedRoles()
    {
        if (!await roleManager.RoleExistsAsync(Authorization.Roles.Admin))
        {
            var adminRole = new ApplicationRole
            {
                Name = Authorization.Roles.Admin,
                NormalizedName = Authorization.Roles.Admin.ToLower(CultureInfo.InvariantCulture),
            };
            await roleManager.CreateAsync(adminRole);
        }

        if (!await roleManager.RoleExistsAsync(Authorization.Roles.User))
        {
            var userRole = new ApplicationRole
            {
                Name = Authorization.Roles.User,
                NormalizedName = Authorization.Roles.User.ToLower(CultureInfo.InvariantCulture),
            };
            await roleManager.CreateAsync(userRole);
        }
    }

    private async Task SeedUsers()
    {
        if (await userManager.FindByEmailAsync("mehdi@test.com") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "mehdi",
                FirstName = "Mehdi",
                LastName = "test",
                Email = "mehdi@test.com",
            };

            var result = await userManager.CreateAsync(user, "123456");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, Authorization.Roles.Admin);
        }
    }
}
