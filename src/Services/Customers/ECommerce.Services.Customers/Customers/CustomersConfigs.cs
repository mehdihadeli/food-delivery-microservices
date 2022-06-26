using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Customers.Customers.Data;

namespace ECommerce.Services.Customers.Customers;

internal class CustomersConfigs : IModuleConfiguration
{
    public const string Tag = "Customers";
    public const string CustomersPrefixUri = $"{CustomersModuleConfiguration.CustomerModulePrefixUri}";

    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddScoped<IDataSeeder, CustomersDataSeeder>();

        return services;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult<WebApplication>(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Here we can add endpoints manually but, if our endpoint inherits from `IMinimalEndpointDefinition`, they discover automatically.
        return endpoints;
    }
}
