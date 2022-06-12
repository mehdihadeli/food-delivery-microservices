namespace ECommerce.Services.Customers.RestockSubscriptions;

public static class RestockSubscriptionsConfigs
{
    public const string Tag = "RestockSubscriptions";

    public const string RestockSubscriptionsUrl =
        $"{CustomersModuleConfiguration.CustomerModulePrefixUri}/restock-subscriptions";

    public static IServiceCollection AddRestockSubscriptionsServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        return services;
    }

    public static IEndpointRouteBuilder MapRestockSubscriptionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
