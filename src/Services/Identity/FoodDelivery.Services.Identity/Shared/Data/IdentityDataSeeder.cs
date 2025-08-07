using System.Globalization;
using System.Security.Claims;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Security;
using FoodDelivery.Services.Identity.Shared.Models;
using FoodDelivery.Services.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Shared.Data;

public class IdentityDataSeeder(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : IDataSeeder<IdentityContext>
{
    public async Task SeedAsync(IdentityContext context)
    {
        await SeedRoles();
        await SeedUsers();
    }

    private async Task SeedRoles()
    {
        // Seed Admin Role with Claims
        if (!await roleManager.RoleExistsAsync(Authorization.Roles.Admin))
        {
            var adminRole = new ApplicationRole
            {
                Name = Authorization.Roles.Admin,
                NormalizedName = Authorization.Roles.Admin.ToLower(CultureInfo.InvariantCulture),
            };

            await roleManager.CreateAsync(adminRole);

            // Add Role Claims (Permissions)
            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsRead)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsWrite)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsFull)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.OrderRead)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.OrderWrite)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.CustomerWrite)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.CustomerRead)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.UserRead)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.UserWrite)
            );

            await roleManager.AddClaimAsync(
                adminRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.GatewayAccess)
            );
        }

        // Seed User Role with Claims
        if (!await roleManager.RoleExistsAsync(Authorization.Roles.User))
        {
            var userRole = new ApplicationRole
            {
                Name = Authorization.Roles.User,
                NormalizedName = Authorization.Roles.User.ToLower(CultureInfo.InvariantCulture),
            };

            await roleManager.CreateAsync(userRole);

            // Add Role Claims (Permissions)
            await roleManager.AddClaimAsync(
                userRole,
                new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsRead)
            );
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
                await userManager.AddToRoleAsync(adminUser, Authorization.Roles.Admin);

                // Add User-info Claims (Override Role Claims if needed)
                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.Type, "management"));

                await userManager.AddClaimAsync(adminUser, new Claim(ClaimsType.CustomAccess, "full"));

                // User-level permissions
                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsRead)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsWrite)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsFull)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.OrderRead)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.OrderWrite)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CustomerRead)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CustomerWrite)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.UserRead)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.UserWrite)
                );

                await userManager.AddClaimAsync(
                    adminUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.GatewayAccess)
                );
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
                await userManager.AddToRoleAsync(regularUser, Authorization.Roles.User);

                // Add User-info Claims
                await userManager.AddClaimAsync(regularUser, new Claim(ClaimsType.Type, "regular"));

                await userManager.AddClaimAsync(regularUser, new Claim(ClaimsType.CustomAccess, "limited"));

                // User-level permissions
                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CatalogsRead)
                );

                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.OrderRead)
                );

                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.CustomerRead)
                );

                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.UserRead)
                );

                await userManager.AddClaimAsync(
                    regularUser,
                    new Claim(ClaimsType.Permission, Authorization.UserPermissions.GatewayAccess)
                );
            }
        }
    }
}
