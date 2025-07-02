namespace FoodDelivery.Services.Customers.RestockSubscriptions;

internal static class RestockSubscriptionsConfigurations
{
    internal const string Tag = "RestockSubscriptions";
    internal const string RestockSubscriptionsUrl =
        $"{ApplicationConfigurations.CustomerModulePrefixUri}/restock-subscriptions";

    internal static WebApplicationBuilder AddRestockSubscriptionsModuleServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    internal static IEndpointRouteBuilder MapRestockSubscriptionsModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
