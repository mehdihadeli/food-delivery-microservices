using Microsoft.AspNetCore.Authentication.Cookies;

namespace FoodDelivery.Spa.Bff.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            // This sets a default policy that applies when no other policy is specified.
            options.AddPolicy(
                "RequireAuthenticatedUserPolicy",
                policy =>
                {
                    policy
                        .RequireAuthenticatedUser()
                        .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                    // policy.RequireClaim(ClaimsType.Scope, Scopes.Gateway);
                    // policy.RequireClaim(ClaimsType.Permission, Permissions.GatewayAccess);
                }
            );
        });

        return builder;
    }
}
