using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Services.Identity.Identity.Data;

public class IdentityDataSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : IDataSeeder
{
    public int Order => 1;

    public async Task SeedAllAsync(CancellationToken cancellationToken)
    {
        await SeedRoles();
        await SeedUsers();
    }

    private async Task SeedRoles()
    {
        if (!await roleManager.RoleExistsAsync(ApplicationRole.Admin.Name))
            await roleManager.CreateAsync(ApplicationRole.Admin);

        if (!await roleManager.RoleExistsAsync(ApplicationRole.User.Name))
            await roleManager.CreateAsync(ApplicationRole.User);
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
                await userManager.AddToRoleAsync(user, ApplicationRole.Admin.Name);
        }

        if (await userManager.FindByEmailAsync("mehdi2@test.com") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "mehdi2",
                FirstName = "Mehdi",
                LastName = "Test",
                Email = "mehdi2@test.com",
            };

            var result = await userManager.CreateAsync(user, "123456");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, ApplicationRole.User.Name);
        }
    }
}
