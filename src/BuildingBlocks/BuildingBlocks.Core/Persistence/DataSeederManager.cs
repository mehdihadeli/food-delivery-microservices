using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Persistence;

public class DataSeederManager(
    IWebHostEnvironment webHostEnvironment,
    ILogger<DataSeederManager> logger,
    IServiceProvider serviceProvider
) : IDataSeederManager
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        if (!webHostEnvironment.IsTest())
        {
            logger.LogInformation("Seeding application data started");
            var appDataSeeder = scope.ServiceProvider.GetServices<IDataSeeder>().ToList();

            // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
            // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
            foreach (var seeder in appDataSeeder.OrderBy(x => x.Order))
            {
                await seeder.SeedAllAsync(cancellationToken);
                logger.LogInformation("Seeding '{Seed}' finished...", seeder.GetType().Name);
            }
        }
        else
        {
            logger.LogInformation("Seeding test data started");
            var testDataSeeders = scope.ServiceProvider.GetServices<ITestDataSeeder>().ToList();

            foreach (var seeder in testDataSeeders.OrderBy(x => x.Order))
            {
                await seeder.SeedAllAsync(cancellationToken);
                logger.LogInformation("Seeding '{Seed}' finished...", seeder.GetType().Name);
            }
        }
    }
}
