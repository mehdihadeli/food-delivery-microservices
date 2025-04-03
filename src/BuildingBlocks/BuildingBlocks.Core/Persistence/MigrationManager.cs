using BuildingBlocks.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Persistence;

// because our migration should apply first we should apply migration before running all background services with our MigrationManager and before `app.RunAsync()` for running host and workers
public class MigrationManager(IServiceProvider serviceProvider, ILogger<MigrationManager> logger) : IMigrationManager
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
        // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
        using var serviceScope = serviceProvider.CreateScope();
        var migrations = serviceScope.ServiceProvider.GetServices<IMigrationSchema>();

        foreach (var migration in migrations)
        {
            logger.LogInformation("Migration '{Migration}' started...", migrations.GetType().Name);
            await migration.ExecuteAsync(cancellationToken);
            logger.LogInformation("Migration '{Migration}' ended...", migration.GetType().Name);
        }
    }
}
