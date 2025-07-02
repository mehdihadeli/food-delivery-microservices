using FoodDelivery.Services.Identity.Identity;
using FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;
using FoodDelivery.Services.Identity.Users;

namespace FoodDelivery.Services.Identity;

public static class ApplicationConfiguration
{
    public const string IdentityModulePrefixUri = "api/v{version:apiVersion}/identity";

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        // Identity service Configurations
        builder.AddIdentityStorage();

        // Modules
        builder.AddUsersModuleServices();
        builder.AddIdentityModuleServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Identity Service Apis.").ExcludeFromDescription();

        endpoints.MapIdentityModuleEndpoints();
        endpoints.MapUsersModuleEndpoints();

        return endpoints;
    }
}
