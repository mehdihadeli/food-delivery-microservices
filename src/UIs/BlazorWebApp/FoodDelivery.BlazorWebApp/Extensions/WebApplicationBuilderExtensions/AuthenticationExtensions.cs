using BuildingBlocks.Core.Extensions;
using LinqKit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using OAuthOptions = BuildingBlocks.Core.Security.OAuthOptions;

namespace FoodDelivery.BlazorWebApp.Extensions.WebApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthentication(this IHostApplicationBuilder builder)
    {
        var oauthOptions = builder.Configuration.BindOptions<OAuthOptions>();

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // DefaultChallengeScheme will use in `HttpContext.ChallengeAsync` in duende.bff and its login endpoint `bff/login` (DefaultLoginService)
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    // https://docs.duendesoftware.com/bff/fundamentals/session/handlers/#the-cookie-handler

                    // strict SameSite handling
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.Name = oauthOptions.CookieName;
                    // set session lifetime
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(oauthOptions.SessionCookieLifetimeMinutes);
                    options.SlidingExpiration = false;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            )
            .AddOpenIdConnect(
                OpenIdConnectDefaults.AuthenticationScheme,
                options =>
                {
                    // https://docs.duendesoftware.com/bff/fundamentals/session/handlers/#the-openid-connect-authentication-handler
                    // proxy url: https://localhost:3001/auth
                    options.Authority = oauthOptions.Authority;
                    // confidential client using code flow
                    options.ClientId = oauthOptions.ClientId;
                    options.ClientSecret = oauthOptions.ClientSecret;
                    options.ResponseType = oauthOptions.ResponseType ?? "code";
                    // query response type is compatible with strict SameSite mode
                    options.ResponseMode = oauthOptions.ResponseMode ?? "query";
                    options.GetClaimsFromUserInfoEndpoint = oauthOptions.GetClaimsFromUserInfoEndpoint;
                    options.MapInboundClaims = oauthOptions.MapInboundClaims;
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                    // PKCE must match IdentityServer
                    options.UsePkce = true;

                    // options.CallbackPath = new PathString("/signin-oidc");
                    // options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
                    // options.RemoteSignOutPath = new PathString("/signout-oidc");

                    // With SaveTokens = true, after successful login, ASP.NET Core will store the tokens in the cookie-authentication ticket, and you can retrieve them `HttpContext.GetTokenAsync("access_token")`
                    options.SaveTokens = oauthOptions.SaveTokens;

                    if (oauthOptions.Scopes.Any())
                    {
                        var scopes = oauthOptions.Scopes;

                        // request scopes
                        options.Scope.Clear();
                        scopes.ForEach(scope => options.Scope.Add(scope));
                    }

                    options.TokenValidationParameters = new() { NameClaimType = "name", RoleClaimType = "role" };
                }
            );

        // Blazor auth services
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        builder.Services.AddCascadingAuthenticationState();

        return builder;
    }
}
