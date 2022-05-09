using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security.Jwt;

public static class Extensions
{
    public static IServiceCollection AddCustomAuthorization(
        this IServiceCollection services,
        IList<ClaimPolicy>? claimPolicies = null,
        IList<RolePolicy>? rolePolicies = null)
    {
        services.AddAuthorization(authorizationOptions =>
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme
            // https://andrewlock.net/setting-global-authorization-policies-using-the-defaultpolicy-and-the-fallbackpolicy-in-aspnet-core-3/
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme);
            defaultAuthorizationPolicyBuilder =
                defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            authorizationOptions.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims
            if (claimPolicies is { })
            {
                foreach (var policy in claimPolicies)
                {
                    authorizationOptions.AddPolicy(policy.Name, x =>
                    {
                        x.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        foreach (var policyClaim in policy.Claims)
                        {
                            x.RequireClaim(policyClaim.Type, policyClaim.Value);
                        }
                    });
                }
            }

            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization
            if (rolePolicies is { })
            {
                foreach (var rolePolicy in rolePolicies)
                {
                    authorizationOptions.AddPolicy(rolePolicy.Name, x =>
                    {
                        x.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                        x.RequireRole(rolePolicy.Roles);
                    });
                }
            }
        });

        return services;
    }
}
