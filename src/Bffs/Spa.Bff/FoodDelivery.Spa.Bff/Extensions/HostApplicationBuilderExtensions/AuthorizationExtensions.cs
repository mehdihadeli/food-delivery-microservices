using Microsoft.AspNetCore.Authorization;

namespace FoodDelivery.Spa.Bff.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            //// This sets a default policy that applies when no other policy is specified.
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            options.AddPolicy(
                "RequireAuthenticatedUserPolicy",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    // policy.RequireClaim(ClaimsType.Scope, Scopes.Gateway);
                    // policy.RequireClaim(ClaimsType.Permission, Permissions.GatewayAccess);
                }
            );
        });

        return builder;
    }
}
