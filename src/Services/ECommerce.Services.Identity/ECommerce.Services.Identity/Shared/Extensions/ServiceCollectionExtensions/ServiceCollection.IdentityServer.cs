using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Builder;
using ECommerce.Services.Identity.Identity.Services;
using ECommerce.Services.Identity.Shared.Models;

// Ref:https://www.scottbrady91.com/identity-server/getting-started-with-identityserver-4
namespace ECommerce.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder)
    {
        AddCustomIdentityServer(builder.Services);

        return builder;
    }

    public static IServiceCollection AddCustomIdentityServer(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, IdentityProfileService>();

        services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddDeveloperSigningCredential() // This is for dev only scenarios when you don’t have a certificate to use.
            .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
            .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
            .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
            .AddInMemoryClients(IdentityServerConfig.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<IdentityProfileService>();

        return services;
    }
}
