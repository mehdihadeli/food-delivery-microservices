namespace FoodDelivery.BlazorWebApp.Extensions.WebApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
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
