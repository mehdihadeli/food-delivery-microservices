using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Identity.Shared;
using ECommerce.Services.Identity.Users.Features.GettingUerByEmail.v1;
using ECommerce.Services.Identity.Users.Features.GettingUserById.v1;
using ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;
using ECommerce.Services.Identity.Users.Features.UpdatingUserState.v1;

namespace ECommerce.Services.Identity.Users;

internal class UsersConfigs : IModuleConfiguration
{
    public const string Tag = "Users";
    public const string UsersPrefixUri = $"{SharedModulesConfiguration.IdentityModulePrefixUri}/users";

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var usersVersionGroup = endpoints
            .MapApiGroup(Tag)
            .WithTags(Tag);

        // create a new sub group for each version
        var usersGroupV1 = usersVersionGroup
            .MapGroup(UsersPrefixUri)
            .HasApiVersion(1.0);

        // create a new sub group for each version
        var usersGroupV2 = usersVersionGroup
            .MapGroup(UsersPrefixUri)
            .HasApiVersion(2.0);

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0#route-groups
        // https://github.com/dotnet/aspnet-api-versioning/blob/main/examples/AspNetCore/WebApi/MinimalOpenApiExample/Program.cs
        usersGroupV1.MapRegisterNewUserEndpoint();
        usersGroupV1.MapUpdateUserStateEndpoint();
        usersGroupV1.MapGetUserByIdEndpoint();
        usersGroupV1.MapGetUserByEmailEndpoint();

        return endpoints;
    }
}
