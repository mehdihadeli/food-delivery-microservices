using Duende.IdentityServer.Services;
using FoodDelivery.Services.Identity.Identity;
using FoodDelivery.Services.Identity.Identity.Services;
using FoodDelivery.Services.Identity.Shared.Models;

// Ref:https://www.scottbrady91.com/identity-server/getting-started-with-identityserver-4
namespace FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomIdentityServer(this IHostApplicationBuilder builder)
    {
        // https://docs.duendesoftware.com/identityserver/quickstarts/5-aspnetid
        builder
            .Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryIdentityResources(Config.GetIdentityResources())
            .AddInMemoryApiResources(Config.GetApiResources())
            .AddInMemoryApiScopes(Config.GetApiScopes())
            .AddInMemoryClients(Config.GetClients(builder.Configuration))
            // - for some flows like implicit-flow and resource owner password require a user store (to validate credentials like username/password). with adding `AddAspNetIdentity<ApplicationUser>` identity server uses stored users for Handles password hashing, 2FA, profile data, etc otherwise we should use `AddTestUsers(TestUsers.Users)` to handling users credential for authentication for clients that need user information like implicitflow
            // 1. IdentityServer checks the ApplicationUser store (database) to validate credentials. 2. IdentityServer includes user claims (identity attributes) in tokens. By default, AddAspNetIdentity<ApplicationUser>() maps basic claims like: sub, name, email, role. we can extend this with a Profile Service (IdentityProfileService in your code) to add custom claims.
            // https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity	//https://learn.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity
            .AddAspNetIdentity<ApplicationUser>();

        // ref: https://github.com/dotnet/eShop/blob/main/src/Identity.API/Program.cs
        builder.Services.AddTransient<IProfileService, IdentityProfileService>();
        builder.Services.AddTransient<ILoginService<ApplicationUser>, EfLoginService>();
        builder.Services.AddTransient<IRedirectService, RedirectService>();

        return builder;
    }
}
