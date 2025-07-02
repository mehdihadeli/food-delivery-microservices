using Asp.Versioning.Builder;

namespace FoodDelivery.Services.Customers.Customers;

internal static class CustomersConfigurations
{
    internal const string Tag = "Customers";
    internal const string CustomersPrefixUri =
        $"{Services.Customers.ApplicationConfigurations.CustomerModulePrefixUri}";
    internal static ApiVersionSet VersionSet { get; private set; } = default!;

    internal static WebApplicationBuilder AddCustomersModuleServices(this WebApplicationBuilder builder)
    {
        return builder;
    }

    public static IEndpointRouteBuilder MapCustomersModuleEndpoints(IEndpointRouteBuilder endpoints)
    {
        VersionSet = endpoints.NewApiVersionSet(Tag).Build();

        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
