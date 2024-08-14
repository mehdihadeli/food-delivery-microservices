using BuildingBlocks.Abstractions.Persistence;
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
        if (!webHostEnvironment.IsEnvironment("test"))
        {
            logger.LogInformation("Seed worker started");

            // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
            // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
            using var serviceScope = serviceScopeFactory.CreateScope();
            var seeders = serviceScope.ServiceProvider.GetServices<IDataSeeder>();

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
        if (!webHostEnvironment.IsEnvironment("test"))
        {
            logger.LogInformation("Seed worker stopped");
        }

        return base.StopAsync(cancellationToken);
    }
}
