using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products;
using ECommerce.Services.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Catalogs.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs;

public class CatalogModuleConfiguration : IRootModuleDefinition
{
    public const string CatalogModulePrefixUri = "api/v1/catalogs";

    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddInfrastructure(configuration, webHostEnvironment);

        services.AddStorage(configuration);

        // Add Sub Modules Services
        services.AddBrandsServices();
        services.AddCategoriesServices();
        services.AddSuppliersServices();

        services.AddProductsServices();

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

            return $"Catalogs Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        // Add Sub Modules Endpoints
        endpoints.MapProductsEndpoints();

        return endpoints;
    }
}
