using FoodDelivery.Services.Identity.Identity;
using FoodDelivery.Services.Identity.Identity.Services;
using FoodDelivery.Services.Identity.Shared.Models;

// Ref:https://www.scottbrady91.com/identity-server/getting-started-with-identityserver-4
namespace FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
            .AddInMemoryApiResources(IdentityServerConfig.ApiResources)
            .AddInMemoryApiScopes(IdentityServerConfig.ApiScopes)
            .AddInMemoryClients(IdentityServerConfig.Clients)
			// - for some flows like implicitflow and resource owner password  require a user store (to validate credentials like username/password). with adding `AddAspNetIdentity<ApplicationUser>` identity server uses stored users for Handles password hashing, 2FA, profile data, etc otherwise we should use `AddTestUsers(TestUsers.Users)` to handling users credential for authentication for clients that need user information like implicitflow
			// 1. IdentityServer checks the ApplicationUser store (database) to validate credentials. 2. IdentityServer includes user claims (identity attributes) in tokens. By default, AddAspNetIdentity<ApplicationUser>() maps basic claims like: sub, name, email, role. we can extend this with a Profile Service (IdentityProfileService in your code) to add custom claims.
			// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity	//https://learn.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<IdentityProfileService>();

        return builder;
    }
}
