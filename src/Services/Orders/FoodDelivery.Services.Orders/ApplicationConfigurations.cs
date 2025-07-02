using FoodDelivery.Services.Orders.Shared.Extensions.HostApplicationBuilderExtensions;

namespace FoodDelivery.Services.Orders;

public static class ApplicationConfigurations
{
    public const string OrderModulePrefixUri = "api/v{version:apiVersion}/orders";

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        // Orders Configurations
        builder.AddStorage();

        // Modules

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Orders Service Api.").ExcludeFromDescription();

        // Modules
        return endpoints;
    }
}
