using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Web.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Persistence;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
// Hint: we can't guarantee execution order of our seeder, and because our migration should apply first we should apply migration before running all background services with our MigrationManager and before `app.RunAsync()` for running host and workers
public class SeedWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SeedWorker> logger,
    IWebHostEnvironment webHostEnvironment
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Seed worker started");

        using var serviceScope = serviceScopeFactory.CreateScope();

        // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
        // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
        var testSeeders = serviceScope.ServiceProvider.GetServices<ITestDataSeeder>();
        var seeders = serviceScope.ServiceProvider.GetServices<IDataSeeder>();
        if (webHostEnvironment.IsTest())
        {
            foreach (var testDataSeeder in testSeeders.OrderBy(x => x.Order))
            {
                logger.LogInformation("Seeding '{Seed}' started...", testDataSeeder.GetType().Name);
                await testDataSeeder.SeedAllAsync();
                logger.LogInformation("Seeding '{Seed}' ended...", testDataSeeder.GetType().Name);
            }
        }
        else
        {
            foreach (var seeder in seeders.OrderBy(x => x.Order))
            {
                logger.LogInformation("Seeding '{Seed}' started...", seeder.GetType().Name);
                await seeder.SeedAllAsync();
                logger.LogInformation("Seeding '{Seed}' ended...", seeder.GetType().Name);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seed worker stopped");

        return base.StopAsync(cancellationToken);
    }
}
