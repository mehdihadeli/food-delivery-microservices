using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogsMigrationSchema(CatalogDbContext catalogsDbContext, ILogger<CatalogsMigrationSchema> logger)
    : IMigrationSchema
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migration worker started");

        logger.LogInformation("Updating catalog database...");

        await catalogsDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Catalog database Updated");
    }
}
