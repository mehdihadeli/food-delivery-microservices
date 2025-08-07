using System.Diagnostics;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Diagnostics.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

// - Using IHostedService for fixing the problem for running this worker in the background with BackgroundService and running late. using IHostedService, we are ensuring our hosted-service executed before ServiceProvider resolve in the tests.
// - Doing migration and seeding during startup can slow down the startup, but that’s often an intentional and acceptable tradeoff for ensuring application correctness and stability. A slightly slower startup is better than a fast crash or corrupt data.
// - If our app starts before the DB schema is up to date, it might crash or behave unpredictably, and the app might serve requests before migration finishes.
// - Benefits of Separating Migration/Seeding into a Worker: Keeps Program.cs clean, reusability with injecting different DbContexts and seed functions via DI, easier to test isolated migration behavior if we want,
public class MigrationSeedWorker<TContext>(IServiceProvider serviceProvider) : IHostedService
    where TContext : DbContext
{
    private static readonly ActivitySource ActivitySource = new(MigrationExtensions.ActivitySourceName);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var scopeServiceProvider = scope.ServiceProvider;
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder<TContext>>();
        var logger = scopeServiceProvider.GetRequiredService<ILogger<TContext>>();
        var context = scopeServiceProvider.GetRequiredService<TContext>();

        using Activity? activity = ActivitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

        try
        {
            logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(() => ExecuteAsync(seeder, context));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while migrating the database used on context {DbContextName}",
                typeof(TContext).Name
            );

            activity?.SetExceptionTags(ex);

            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task ExecuteAsync<TContext>(IDataSeeder<TContext> seeder, TContext context)
        where TContext : DbContext
    {
        using var activity = ActivitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        try
        {
            await context.Database.MigrateAsync();
            await seeder.SeedAsync(context);
        }
        catch (Exception ex)
        {
            activity?.SetExceptionTags(ex);

            throw;
        }
    }
}
