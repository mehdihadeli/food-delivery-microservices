using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogsMigrationExecutor : IMigrationExecutor
{
    private readonly CatalogDbContext _catalogDbContext;
    private readonly ILogger<CatalogsMigrationExecutor> _logger;

    public CatalogsMigrationExecutor(CatalogDbContext catalogDbContext, ILogger<CatalogsMigrationExecutor> logger)
    {
        _catalogDbContext = catalogDbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration worker started");

        _logger.LogInformation("Updating catalog database...");

        await _catalogDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("catalog database Updated");
    }
}
