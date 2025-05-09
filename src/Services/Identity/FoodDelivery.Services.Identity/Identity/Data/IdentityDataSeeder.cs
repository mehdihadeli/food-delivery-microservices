using System.Security.Claims;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Security;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Shared;
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
        var userRole = ApplicationRole.UserRole;
        var adminRole = ApplicationRole.AdminRole;

        // Seed Admin Role with Claims
        if (!await roleManager.RoleExistsAsync(adminRole.Name))
        {
            await roleManager.CreateAsync(adminRole);

            // Add Role Claims (Permissions)
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.CatalogsRead));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.CatalogsWrite));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.CatalogsFull));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.OrderRead));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.OrderWrite));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.CustomerWrite));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.CustomerRead));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.UserRead));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.UserWrite));
            await roleManager.AddClaimAsync(adminRole, new Claim(ClaimsType.Permission, Permissions.GatewayAccess));
        }

        // Seed User Role with Claims
        if (!await roleManager.RoleExistsAsync(userRole.Name))
        {
            await roleManager.CreateAsync(userRole);

            // Add Role Claims (Permissions)
            await roleManager.AddClaimAsync(userRole, new Claim(ClaimsType.Permission, Permissions.CatalogsRead));
        }
    }

    private async Task SeedUsers()
    {
        // Seed Admin User with Additional Claims
        if (await userManager.FindByEmailAsync("mehdi@test.com") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "mehdi",
                FirstName = "Mehdi",
                LastName = "test",
                Email = "mehdi@test.com",
            };

            var result = await userManager.CreateAsync(adminUser, "123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, ApplicationRole.AdminRole.Name);

                // Add User-info Claims (Override Role Claims if needed)
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Type, "management"));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.CustomAccess, "full"));

                // User-level permissions
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.CatalogsRead));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.CatalogsWrite));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.CatalogsFull));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.OrderRead));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.OrderWrite));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.CustomerRead));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.CustomerWrite));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.UserRead));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.UserWrite));
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Permission, Permissions.GatewayAccess));
            }
        }

        // Seed Regular User with Additional Claims
        if (await userManager.FindByEmailAsync("mehdi2@test.com") == null)
        {
            var regularUser = new ApplicationUser
            {
                UserName = "mehdi2",
                FirstName = "Mehdi",
                LastName = "Test",
                Email = "mehdi2@test.com",
            };

            var result = await userManager.CreateAsync(regularUser, "123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, ApplicationRole.UserRole.Name);

                // Add User-info Claims
                await userManager.AddClaimAsync(regularUser, new Claim(ClaimsType.Type, "regular"));
                await userManager.AddClaimAsync(regularUser, new Claim(ClaimsType.CustomAccess, "limited"));

                // User-level permissions
                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Permissions.CatalogsRead)
                );
                await userManager.AddClaimAsync(regularUser, new Claim(ClaimsType.Permission, Permissions.OrderRead));
                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Permissions.CustomerRead)
                );
                await userManager.AddClaimAsync(regularUser, new Claim(ClaimsType.Permission, Permissions.UserRead));
                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Permissions.GatewayAccess)
                );
            }
        }
    }
}
