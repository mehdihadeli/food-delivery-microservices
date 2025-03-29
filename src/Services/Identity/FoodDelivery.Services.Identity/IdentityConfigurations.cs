using FoodDelivery.Services.Identity.Identity;
using FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationBuilderExtensions;
using FoodDelivery.Services.Identity.Users;

namespace FoodDelivery.Services.Identity;

public static class IdentityConfigurations
{
    public const string IdentityModulePrefixUri = "api/v{version:apiVersion}/identity";

    public static WebApplicationBuilder AddIdentityServices(this WebApplicationBuilder builder)
    {
        // Identity service Configurations
        builder.AddIdentityStorage();

        // Modules
        builder.AddUsersModuleServices();
        builder.AddIdentityModuleServices();

        return builder;
    }

    public static WebApplication UseIdentity(this WebApplication app)
    {
        // Modules
        app.UseIdentityModule();
        app.UseUsersModule();

        return app;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Identity Service Apis.").ExcludeFromDescription();

        endpoints.MapIdentityModuleEndpoints();
        endpoints.MapUsersModuleEndpoints();

        return endpoints;
    }
}
