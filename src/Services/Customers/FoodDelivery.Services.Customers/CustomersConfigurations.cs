using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.Shared.Extensions.WebApplicationBuilderExtensions;

namespace FoodDelivery.Services.Customers;

public static class CustomersConfigurations
{
    public const string CustomerModulePrefixUri = "api/v{version:apiVersion}/customers";

    public static WebApplicationBuilder AddCustomersServices(this WebApplicationBuilder builder)
    {
        // Customers Configurations
        builder.AddStorage();

        // Modules
        builder.AddCustomersModuleServices();
        builder.AddRestockSubscriptionsModuleServices();

        return builder;
    }

    public static WebApplication UseCustomers(this WebApplication app)
    {
        // Modules
        app.UseCustomersModule();
        app.UseRestockSubscriptionsModule();

        return app;
    }

    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Customers Service Api.").ExcludeFromDescription();

        return endpoints;
    }
}
