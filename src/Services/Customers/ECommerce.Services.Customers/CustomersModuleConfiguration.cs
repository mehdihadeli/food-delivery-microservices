using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Customers.Customers;
using ECommerce.Services.Customers.RestockSubscriptions;
using ECommerce.Services.Customers.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Services.Customers;

public class CustomersModuleConfiguration : IRootModuleDefinition
{
    public const string CustomerModulePrefixUri = "api/v1/customers";

    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddInfrastructure(configuration, webHostEnvironment);

        services.AddStorage(configuration);

        // Add Sub Modules Services
        services.AddCustomersServices(configuration, webHostEnvironment);

        services.AddRestockSubscriptionsServices(configuration, webHostEnvironment);

        return services;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        ServiceActivator.Configure(app.Services);

        if (app.Environment.IsEnvironment("test") == false)
            app.UseMonitoring();

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        return app;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Customers Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        // Add Sub Modules Endpoints
        endpoints.MapCustomersEndpoints();

        endpoints.MapRestockSubscriptionsEndpoints();

        return endpoints;
    }
}
