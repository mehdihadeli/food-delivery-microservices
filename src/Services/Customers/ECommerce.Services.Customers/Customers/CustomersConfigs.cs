using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Customers.Customers.Data;
using ECommerce.Services.Customers.Shared;

namespace ECommerce.Services.Customers.Customers;

internal class CustomersConfigs : IModuleConfiguration
{
    public const string Tag = "Customers";
    public const string CustomersPrefixUri = $"{SharedModulesConfiguration.CustomerModulePrefixUri}";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDataSeeder, CustomersDataSeeder>();
        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult<WebApplication>(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        VersionSet = endpoints.NewApiVersionSet(Tag).Build();

        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
