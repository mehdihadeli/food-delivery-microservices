using BuildingBlocks.Core.Security;
using FoodDelivery.Services.Shared;

namespace FoodDelivery.Services.Orders.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Authorization.UserPermissions.OrderRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.OrdersRead,
                        Authorization.Scopes.OrdersFull
                    );
                    // Check for user permission
                    policy.RequireClaim(
                        ClaimsType.Permission,
                        Authorization.UserPermissions.OrderRead,
                        Authorization.UserPermissions.OrderFull
                    );
                }
            );

            options.AddPolicy(
                Authorization.UserPermissions.OrderWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.OrdersWrite,
                        Authorization.Scopes.OrdersFull
                    );
                    // Check for user permission
                    policy.RequireClaim(
                        ClaimsType.Permission,
                        Authorization.UserPermissions.OrderWrite,
                        Authorization.UserPermissions.OrderFull
                    );
                }
            );

            // Role-based policies
            options.AddPolicy(
                Authorization.Roles.Admin,
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
