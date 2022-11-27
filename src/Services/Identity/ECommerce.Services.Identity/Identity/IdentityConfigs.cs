using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Extensions;
using ECommerce.Services.Identity.Identity.Data;
using ECommerce.Services.Identity.Identity.Features.GettingClaims.v1;
using ECommerce.Services.Identity.Identity.Features.Login.v1;
using ECommerce.Services.Identity.Identity.Features.Logout.v1;
using ECommerce.Services.Identity.Identity.Features.RefreshingToken.v1;
using ECommerce.Services.Identity.Identity.Features.RevokingAccessToken.v1;
using ECommerce.Services.Identity.Identity.Features.RevokingRefreshToken.v1;
using ECommerce.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;
using ECommerce.Services.Identity.Identity.Features.VerifyingEmail.v1;
using ECommerce.Services.Identity.Shared;
using ECommerce.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ECommerce.Services.Identity.Identity;

internal class IdentityConfigs : IModuleConfiguration
{
    public const string Tag = "Identity";
    public const string IdentityPrefixUri = $"{SharedModulesConfiguration.IdentityModulePrefixUri}";

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.AddCustomIdentity();

        builder.Services.AddScoped<IDataSeeder, IdentityDataSeeder>();

        if (builder.Environment.IsTest() == false)
            builder.AddCustomIdentityServer();

        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var identityVersionGroup = endpoints
            .MapApiGroup(Tag)
            .WithTags(Tag);

        // create a new sub group for each version
        var identityGroupV1 = identityVersionGroup
            .MapGroup(IdentityPrefixUri)
            .HasApiVersion(1.0);

        // create a new sub group for each version
        var identityGroupV2 = identityVersionGroup
            .MapGroup(IdentityPrefixUri)
            .HasApiVersion(2.0);

        identityGroupV1.MapGet(
            "/user-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = IdentityConstants.Role.User)]
            () => new {Role = IdentityConstants.Role.User}).WithTags("Identity");

        identityGroupV1.MapGet(
            "/admin-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = IdentityConstants.Role.Admin)]
            () => new {Role = IdentityConstants.Role.Admin}).WithTags("Identity");

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#route-groups
        // https://github.com/dotnet/aspnet-api-versioning/blob/main/examples/AspNetCore/WebApi/MinimalOpenApiExample/Program.cs
        identityGroupV1.MapLoginUserEndpoint();
        identityGroupV1.MapLogoutEndpoint();
        identityGroupV1.MapSendEmailVerificationCodeEndpoint();
        identityGroupV1.MapSendVerifyEmailEndpoint();
        identityGroupV1.MapRefreshTokenEndpoint();
        identityGroupV1.MapRevokeTokenEndpoint();
        identityGroupV1.MapRevokeAccessTokenEndpoint();
        identityGroupV1.MapGetClaimsEndpoint();

        return endpoints;
    }
}
