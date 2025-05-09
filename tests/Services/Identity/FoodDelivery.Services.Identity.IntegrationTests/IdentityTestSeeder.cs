using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Identity.Shared.Models;
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
        if (!await roleManager.RoleExistsAsync(ApplicationRole.AdminRole.Name))
            await roleManager.CreateAsync(ApplicationRole.AdminRole);

        if (!await roleManager.RoleExistsAsync(ApplicationRole.UserRole.Name))
            await roleManager.CreateAsync(ApplicationRole.UserRole);
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
                await userManager.AddToRoleAsync(user, ApplicationRole.AdminRole.Name);
        }
    }
}
