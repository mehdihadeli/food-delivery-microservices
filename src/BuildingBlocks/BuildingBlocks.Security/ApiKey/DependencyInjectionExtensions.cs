using BuildingBlocks.Security.ApiKey.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Security.ApiKey;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddCustomApiKeyAuthentication(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
            })
            .AddApiKeySupport(options => { });

        builder.Services.AddAuthorization(options =>
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

        builder.Services.TryAddSingleton<IAuthorizationHandler, OnlyCustomersAuthorizationHandler>();
        builder.Services.TryAddSingleton<IAuthorizationHandler, OnlyAdminsAuthorizationHandler>();
        builder.Services.TryAddSingleton<IAuthorizationHandler, OnlyThirdPartiesAuthorizationHandler>();

        builder.Services.TryAddSingleton<IGetApiKeyQuery, InMemoryGetApiKeyQuery>();

        return builder;
    }
}
