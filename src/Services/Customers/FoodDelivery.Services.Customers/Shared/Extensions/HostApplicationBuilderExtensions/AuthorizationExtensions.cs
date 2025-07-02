using BuildingBlocks.Core.Security;
using FoodDelivery.Services.Shared;

namespace FoodDelivery.Services.Customers.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Authorization.UserPermissions.CustomerRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.CustomersRead,
                        Authorization.Scopes.CustomersFull
                    );
                    // Check for user permission
                    policy.RequireClaim(
                        ClaimsType.Permission,
                        Authorization.UserPermissions.CustomerRead,
                        Authorization.UserPermissions.CustomerFull
                    );
                }
            );

            options.AddPolicy(
                Authorization.UserPermissions.CustomerWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.CustomersWrite,
                        Authorization.Scopes.CustomersFull
                    );
                    // Check for user permission
                    policy.RequireClaim(
                        ClaimsType.Permission,
                        Authorization.UserPermissions.CustomerWrite,
                        Authorization.UserPermissions.CustomerFull
                    );
                }
            );

            // Role-based policies
            options.AddPolicy(
                Authorization.Policies.AdminPolicy,
                x =>
                {
                    x.RequireRole(Authorization.Roles.Admin);
                }
            );
            options.AddPolicy(
                Authorization.Roles.User,
                x =>
                {
                    x.RequireRole(Authorization.Roles.User);
                }
            );
        });

        return builder;
    }
}
