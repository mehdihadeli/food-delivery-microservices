using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Security;
using LinqKit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace FoodDelivery.Spa.Bff.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddCustomAuthentication(this IHostApplicationBuilder builder)
    {
        var oauthOptions = builder.Configuration.BindOptions<OAuthOptions>();

        // https://docs.duendesoftware.com/accesstokenmanagement/web-apps/
        // adds services for token management
        builder.Services.AddOpenIdConnectAccessTokenManagement();

        // https://wrapt.dev/blog/standalone-duende-bff-for-any-spa
        // https://docs.duendesoftware.com/bff/fundamentals/session/handlers/
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
                    options.Authority = oauthOptions.Authority; // https://demo.duendesoftware.com/
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

                    // save tokens into authentication session
                    // to enable automatic token management
                    options.SaveTokens = oauthOptions.SaveTokens;

                    if (oauthOptions.Scopes.Count != 0)
                    {
                        // request scopes
                        options.Scope.Clear();
                        oauthOptions.Scopes.ForEach(scope => options.Scope.Add(scope));
                    }

                    options.TokenValidationParameters = new() { NameClaimType = "name", RoleClaimType = "role" };
                }
            );

        return builder;
    }
}
