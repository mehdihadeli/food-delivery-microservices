using BuildingBlocks.Abstractions.Persistence;

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

            foreach (var seeder in seeders.OrderBy(x => x.Order))
            {
                logger.LogInformation("Seeding '{Seed}' started...", seeder.GetType().Name);
                await seeder.SeedAllAsync();
                logger.LogInformation("Seeding '{Seed}' ended...", seeder.GetType().Name);
            }
        }
    }
}
