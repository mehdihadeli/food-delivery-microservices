using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Security.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Security.Extensions;

public static class Extensions
{
    public static AuthenticationBuilder AddCustomJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtOptions>? configurator = null
    )
    {
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/415
        // https://mderriey.com/2019/06/23/where-are-my-jwt-claims/
        // https://leastprivilege.com/2017/11/15/missing-claims-in-the-asp-net-core-2-openid-connect-handler/
        // https://stackoverflow.com/a/50012477/581476
        // to compatibility with new versions of claim names standard
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var jwtOptions = configuration.BindOptions<JwtOptions>();
        configurator?.Invoke(jwtOptions);

        // add option to the dependency injection
        services.AddValidationOptions<JwtOptions>(opt => configurator?.Invoke(opt));

        services.TryAddTransient<IJwtService, JwtService>();

        // https://docs.microsoft.com/en-us/aspnet/core/security/authentication
        // https://learn.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-6.0#use-multiple-authentication-schemes
        // https://auth0.com/blog/whats-new-in-dotnet-7-for-authentication-and-authorization/
        // since .NET 7, the default scheme is no longer required, when we define just one authentication scheme and It is automatically inferred
        return services
            .AddAuthentication() // no default scheme specified
            .AddJwtBearer(options =>
            {
                //-- JwtBearerDefaults.AuthenticationScheme --
                options.Audience = jwtOptions.Audience;
                options.SaveToken = true;
                options.RefreshOnIssuerKeyNotFound = false;
                options.RequireHttpsMetadata = false;
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    SaveSigninToken = true,
                    // default skew is 5 minutes,
                    // The ClockSkew property allows you to specify the amount of leeway to account for any differences in clock times between the token issuer and the token validation server. This property defines the maximum amount of time (in seconds) by which the token's expiration or not-before time can differ from the system clock on the validation server.
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            throw new UnAuthorizedException("The Token is expired.");
                        }

                        throw new IdentityException(
                            context.Exception.Message,
                            statusCode: StatusCodes.Status500InternalServerError
                        );
                    },
                    OnChallenge = context =>
                    {
                        // context.HandleResponse();
                        // if (!context.Response.HasStarted)
                        // {
                        //     throw new IdentityException(
                        //         "You are not Authorized.",
                        //         statusCode: HttpStatusCode.Unauthorized);
                        // }

                        return Task.CompletedTask;
                    },
                    OnForbidden = _ => throw new ForbiddenException("You are not authorized to access this resource.")
                };
            });
    }

    public static IServiceCollection AddCustomAuthorization(
        this IServiceCollection services,
        IList<ClaimPolicy>? claimPolicies = null,
        IList<RolePolicy>? rolePolicies = null,
        string scheme = JwtBearerDefaults.AuthenticationScheme
    )
    {
        services.AddAuthorization(authorizationOptions =>
        {
            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme
            // https://andrewlock.net/setting-global-authorization-policies-using-the-defaultpolicy-and-the-fallbackpolicy-in-aspnet-core-3/
            var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(scheme);
            defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
            authorizationOptions.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims
            if (claimPolicies is { })
            {
                foreach (var policy in claimPolicies)
                {
                    authorizationOptions.AddPolicy(
                        policy.Name,
                        x =>
                        {
                            x.AuthenticationSchemes.Add(scheme);
                            foreach (var policyClaim in policy.Claims)
                            {
                                x.RequireClaim(policyClaim.Type, policyClaim.Value);
                            }
                        }
                    );
                }
            }

            // https://docs.microsoft.com/en-us/aspnet/core/security/authorization
            if (rolePolicies is { })
            {
                foreach (var rolePolicy in rolePolicies)
                {
                    authorizationOptions.AddPolicy(
                        rolePolicy.Name,
                        x =>
                        {
                            x.AuthenticationSchemes.Add(scheme);
                            x.RequireRole(rolePolicy.Roles);
                        }
                    );
                }
            }
        });

        return services;
    }

    public static void AddExternalLogins(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.BindOptions<JwtOptions>(nameof(JwtOptions));
        jwtOptions.NotBeNull();

        if (jwtOptions.GoogleLoginConfigs is { })
        {
            services
                .AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = jwtOptions.GoogleLoginConfigs.ClientId;
                    googleOptions.ClientSecret = jwtOptions.GoogleLoginConfigs.ClientId;
                    googleOptions.SaveTokens = true;
                });
        }
    }
}
