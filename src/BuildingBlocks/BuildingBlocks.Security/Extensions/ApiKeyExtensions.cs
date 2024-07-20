using BuildingBlocks.Security.ApiKey;
using BuildingBlocks.Security.ApiKey.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Security.Extensions;

public static class ApiKeyExtensions
{
    public static IServiceCollection AddCustomApiKeyAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            })
            .AddApiKeySupport(options => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Policies.OnlyCustomers,
                policy => policy.Requirements.Add(new OnlyCustomersRequirement())
            );
            options.AddPolicy(Policies.OnlyAdmins, policy => policy.Requirements.Add(new OnlyAdminsRequirement()));
            options.AddPolicy(
                Policies.OnlyThirdParties,
                policy => policy.Requirements.Add(new OnlyThirdPartiesRequirement())
            );
        });

        services.TryAddSingleton<IAuthorizationHandler, OnlyCustomersAuthorizationHandler>();
        services.TryAddSingleton<IAuthorizationHandler, OnlyAdminsAuthorizationHandler>();
        services.TryAddSingleton<IAuthorizationHandler, OnlyThirdPartiesAuthorizationHandler>();

        services.TryAddSingleton<IGetApiKeyQuery, InMemoryGetApiKeyQuery>();

        return services;
    }
}
