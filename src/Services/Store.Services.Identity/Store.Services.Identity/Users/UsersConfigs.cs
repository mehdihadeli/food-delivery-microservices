using Store.Services.Identity.Users.Features.GettingUerByEmail;
using Store.Services.Identity.Users.Features.GettingUserById;
using Store.Services.Identity.Users.Features.RegisteringUser;
using Store.Services.Identity.Users.Features.UpdatingUserState;
using Store.Services.Shared.Identity.Users.Events.Integration;
using Humanizer;
using MassTransit;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Store.Services.Identity.Users;

internal static class UsersConfigs
{
    public const string Tag = "Users";
    public const string UsersPrefixUri = $"{IdentityModuleConfiguration.IdentityModulePrefixUri}/users";

    internal static IServiceCollection AddUsersServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }

    internal static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRegisterNewUserEndpoint();
        endpoints.MapUpdateUserStateEndpoint();
        endpoints.MapGetUserByIdEndpoint();
        endpoints.MapGetUserByEmailEndpoint();

        return endpoints;
    }
}
