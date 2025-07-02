using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.Shared.Extensions.HostApplicationBuilderExtensions;

namespace FoodDelivery.Services.Customers;

public static class ApplicationConfigurations
{
    public const string CustomerModulePrefixUri = "api/v{version:apiVersion}/customers";

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        // Customers Configurations
        builder.AddStorage();

        // Modules
        builder.AddCustomersModuleServices();
        builder.AddRestockSubscriptionsModuleServices();

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) => "Customers Service Api.").ExcludeFromDescription();

        return endpoints;
    }
}
