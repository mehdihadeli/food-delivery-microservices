using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Brands.Data;
using ECommerce.Services.Catalogs.Categories.Data;
using ECommerce.Services.Catalogs.Products.Data;
using ECommerce.Services.Catalogs.Suppliers.Data;

namespace ECommerce.Services.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task SeedData(this IApplicationBuilder app, ILogger logger, IWebHostEnvironment environment)
    {
        if (!environment.IsEnvironment("test"))
        {
            // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
            // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
            using var serviceScope = app.ApplicationServices.CreateScope();
            var seeders = serviceScope.ServiceProvider.GetServices<IDataSeeder>();

            logger.LogInformation("Seeding Category Services started...");

            // brand and category should work before product
            var brandSeeder = seeders.First(x => x.GetType().Name == nameof(BrandDataSeeder));
            await brandSeeder.SeedAllAsync();

            var categorySeeder = seeders.First(x => x.GetType().Name == nameof(CategoryDataSeeder));
            await categorySeeder.SeedAllAsync();

            var supplierSeeder = seeders.First(x => x.GetType().Name == nameof(SupplierDataSeeder));
            await supplierSeeder.SeedAllAsync();

            var productSeeder = seeders.First(x => x.GetType().Name == nameof(ProductDataSeeder));
            await productSeeder.SeedAllAsync();

            logger.LogInformation("Seeding Category Services ended...");

        }
    }
}
