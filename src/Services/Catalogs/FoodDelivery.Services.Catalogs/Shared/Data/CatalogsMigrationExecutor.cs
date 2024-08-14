using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogsMigrationExecutor(CatalogDbContext catalogDbContext, ILogger<CatalogsMigrationExecutor> logger)
    : IMigrationExecutor
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migration worker started");

        logger.LogInformation("Updating catalog database...");

        await catalogDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        logger.LogInformation("catalog database Updated");
    }
}
