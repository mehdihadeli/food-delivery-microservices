using FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationBuilderExtensions;

namespace FoodDelivery.Services.Orders;

public static class OrdersConfigurations
{
    public const string OrderModulePrefixUri = "api/v{version:apiVersion}/orders";

    public static WebApplicationBuilder AddOrdersServices(this WebApplicationBuilder builder)
    {
        // Orders Configurations
        builder.AddStorage();

        // Modules

        return builder;
    }

    public static WebApplication UseOrders(this WebApplication app)
    {
        // Modules

        return app;
    }

    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Orders Service Api.").ExcludeFromDescription();

        // Modules
        return endpoints;
    }
}
