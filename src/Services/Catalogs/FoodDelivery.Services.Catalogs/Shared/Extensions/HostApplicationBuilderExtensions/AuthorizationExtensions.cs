using BuildingBlocks.Core.Security;
using FoodDelivery.Services.Shared;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Authorization.Policies.CatalogsReadPolicy,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.CatalogsRead,
                        Authorization.Scopes.CatalogsFull
                    );
                    // Check for user permission
                    policy.RequireClaim(
                        ClaimsType.Permission,
                        Authorization.UserPermissions.CatalogsRead,
                        Authorization.UserPermissions.CatalogsFull
                    );
                }
            );

            options.AddPolicy(
                Authorization.Policies.CatalogsWritePolicy,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // Check for client scope
                    policy.RequireClaim(
                        ClaimsType.Scope,
                        Authorization.Scopes.CatalogsWrite,
                        Authorization.Scopes.CatalogsFull
                    );
                    // Check for user permission
                    policy.RequireClaim(
                        ClaimsType.Permission,
                        Authorization.UserPermissions.CatalogsWrite,
                        Authorization.UserPermissions.CatalogsFull
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
                Authorization.Policies.UserPolicy,
                x =>
                {
                    x.RequireRole(Authorization.Roles.User);
                }
            );
        });

        return builder;
    }
}
