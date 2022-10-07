using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Identity.Identity.Data;
using ECommerce.Services.Identity.Identity.Features.GettingClaims;
using ECommerce.Services.Identity.Identity.Features.Login;
using ECommerce.Services.Identity.Identity.Features.Logout;
using ECommerce.Services.Identity.Identity.Features.RefreshingToken;
using ECommerce.Services.Identity.Identity.Features.RevokingAccessToken;
using ECommerce.Services.Identity.Identity.Features.RevokingRefreshToken;
using ECommerce.Services.Identity.Identity.Features.SendingEmailVerificationCode;
using ECommerce.Services.Identity.Identity.Features.VerifyingEmail;
using ECommerce.Services.Identity.Shared;
using ECommerce.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ECommerce.Services.Identity.Identity;

internal class IdentityConfigs : IModuleConfiguration
{
    public const string Tag = "Identity";
    public const string IdentityPrefixUri = $"{IdentityModuleConfiguration.IdentityModulePrefixUri}";

    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddCustomIdentity(configuration);

        services.AddScoped<IDataSeeder, IdentityDataSeeder>();

        if (webHostEnvironment.IsEnvironment("test") == false)
            services.AddCustomIdentityServer();

        return services;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            $"{IdentityPrefixUri}/user-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = IdentityConstants.Role.User)]
            () => new {Role = IdentityConstants.Role.User}).WithTags("Identity");

        endpoints.MapGet(
            $"{IdentityPrefixUri}/admin-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = IdentityConstants.Role.Admin)]
            () => new {Role = IdentityConstants.Role.Admin}).WithTags("Identity");

        endpoints.MapLoginUserEndpoint();
        endpoints.MapLogoutEndpoint();
        endpoints.MapSendEmailVerificationCodeEndpoint();
        endpoints.MapSendVerifyEmailEndpoint();
        endpoints.MapRefreshTokenEndpoint();
        endpoints.MapRevokeTokenEndpoint();
        endpoints.MapRevokeAccessTokenEndpoint();
        endpoints.MapGetClaimsEndpoint();

        return endpoints;
    }
}
