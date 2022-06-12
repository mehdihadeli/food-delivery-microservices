using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Tests.Shared.Mocks;

namespace Tests.Shared.Extensions;

// Ref: https://blog.joaograssi.com/posts/2021/asp-net-core-testing-permission-protected-api-endpoints/
public static class AuthServiceCollectionExtensions
{
    //https://blog.joaograssi.com/posts/2021/asp-net-core-testing-permission-protected-api-endpoints/
    public static AuthenticationBuilder AddTestAuthentication(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(Constants.AuthConstants.Scheme)
                .RequireAuthenticatedUser()
                .Build();
        });

        return services.AddAuthentication(Constants.AuthConstants.Scheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                Constants.AuthConstants.Scheme, options => { });
    }

    //https://stebet.net/mocking-jwt-tokens-in-asp-net-core-integration-tests/
    public static IServiceCollection AddAuthenticationWithFakeToken(this IServiceCollection services)
    {
        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var config = new OpenIdConnectConfiguration() {Issuer = MockJwtTokens.Issuer};

            config.SigningKeys.Add(MockJwtTokens.SecurityKey);
            options.Configuration = config;
        });

        return services;
    }
}
