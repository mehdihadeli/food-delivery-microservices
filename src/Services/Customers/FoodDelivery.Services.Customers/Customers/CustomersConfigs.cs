using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using FoodDelivery.Services.Customers.Shared;

namespace FoodDelivery.Services.Customers.Customers;

internal class CustomersConfigs : IModuleConfiguration
{
    public const string Tag = "Customers";
    public const string CustomersPrefixUri = $"{SharedModulesConfiguration.CustomerModulePrefixUri}";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        //// we could add event mappers manually, also they can find automatically by scanning assemblies
        // builder.Services.TryAddSingleton<IIntegrationEventMapper, CustomersEventMapper>();
        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        VersionSet = endpoints.NewApiVersionSet(Tag).Build();

        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
