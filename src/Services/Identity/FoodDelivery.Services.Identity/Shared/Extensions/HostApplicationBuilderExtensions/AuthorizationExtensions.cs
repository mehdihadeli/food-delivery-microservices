using BuildingBlocks.Core.Security;
using FoodDelivery.Services.Shared;

namespace FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            // Users related operations only available for identity server clients with having user scope like client-credentials and are not available for users so here we just use scope-based permission for clients not user's based permission claims
            options.AddPolicy(
                Authorization.ClientPermissions.UserRead,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.UsersRead,
                        Authorization.Scopes.UsersFull
                    );
                }
            );

            options.AddPolicy(
                Authorization.ClientPermissions.UserWrite,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.UsersWrite,
                        Authorization.Scopes.UsersFull
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
