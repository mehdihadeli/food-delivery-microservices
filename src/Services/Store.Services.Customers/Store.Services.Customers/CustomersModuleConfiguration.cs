using BuildingBlocks.Abstractions.Web.Module;
using Store.Services.Customers.Shared.Extensions.ApplicationBuilderExtensions;
using Store.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

namespace Store.Services.Customers;

public class CustomersModuleConfiguration : IRootModuleDefinition
{
    public const string CustomerModulePrefixUri = "api/v1/customers";

    public IServiceCollection AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddCustomHttpClients(configuration);
        services.AddStorage(configuration);

        return services;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        await app.UseInfrastructure(app.Logger);

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

        return endpoints;
    }
}
