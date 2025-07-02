using FoodDelivery.Services.Identity.Identity.Features.GettingClaims.v1;
using FoodDelivery.Services.Identity.Identity.Features.SendingEmailVerificationCode.v1;
using FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1;

namespace FoodDelivery.Services.Identity.Identity;

internal static class IdentityConfigurations
{
    public const string Tag = "Identity";
    public const string IdentityPrefixUri = $"{ApplicationConfiguration.IdentityModulePrefixUri}";

    internal static WebApplicationBuilder AddIdentityModuleServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapIdentityModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnet-api-versioning/commit/b789e7e980e83a7d2f82ce3b75235dee5e0724b4
        // changed from MapApiGroup to NewVersionedApi in v7.0.0
        var routeCategoryName = Tag;
        var identityVersionGroup = endpoints.NewVersionedApi(name: routeCategoryName).WithTags(Tag);

        // create a new subgroup for v1 version
        var identityGroupV1 = identityVersionGroup.MapGroup(IdentityPrefixUri).HasApiVersion(1.0);

        // create a new subgroup for v2 version/
        var identityGroupV2 = identityVersionGroup.MapGroup(IdentityPrefixUri).HasApiVersion(2.0);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#route-groups
        // https://github.com/dotnet/aspnet-api-versioning/blob/main/examples/AspNetCore/WebApi/MinimalOpenApiExample/Program.cs
        identityGroupV1.MapSendEmailVerificationCodeEndpoint();
        identityGroupV1.MapSendVerifyEmailEndpoint();
        identityGroupV1.MapGetClaimsEndpoint();

        return endpoints;
    }
}
