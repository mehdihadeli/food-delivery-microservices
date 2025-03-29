using System.Diagnostics;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Diagnostics;
using BuildingBlocks.Core.Diagnostics.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Persistence;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
// Here we use `IHostedService` instead of `BackgroundService` because we want to have control for running async task in StartAsync method and wait for completion not running it in background like `BackgroundService` in its StartAsync
public class MigrationWorker(IMigrationManager migrationManager, ILogger<MigrationWorker> logger) : IHostedService
{
    public static ActivitySource ActivitySource { get; } = new("DbMigrations");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Migration operation");

        try
        {
            await migrationManager.ExecuteAsync(cancellationToken);
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating");

            activity?.SetExceptionTags(ex);

            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
