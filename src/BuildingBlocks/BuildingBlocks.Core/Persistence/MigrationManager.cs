using BuildingBlocks.Abstractions.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Persistence;

// because our migration should apply first we should apply migration before running all background services with our MigrationManager and before `app.RunAsync()` for running host and workers
public class MigrationManager : IMigrationManager
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MigrationManager> _logger;
    private readonly IWebHostEnvironment _environment;

    public MigrationManager(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MigrationManager> logger,
        IWebHostEnvironment environment
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _environment = environment;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
        // https://www.michalbialecki.com/2020/07/20/adding-entity-framework-core-5-migrations-to-net-5-project/
        using var serviceScope = _serviceScopeFactory.CreateScope();
        var migrations = serviceScope.ServiceProvider.GetServices<IMigrationExecutor>();

        foreach (var migration in migrations)
        {
            _logger.LogInformation("Migration '{Migration}' started...", migrations.GetType().Name);
            await migration.ExecuteAsync(stoppingToken);
            _logger.LogInformation("Migration '{Migration}' ended...", migration.GetType().Name);
        }
    }
}
