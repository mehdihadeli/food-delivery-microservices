using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Identity.Shared.Data;

public class IdentityMigrationSchema(IdentityContext identityContext, ILogger<IdentityMigrationSchema> logger)
    : IMigrationSchema
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migration worker started");

        logger.LogInformation("Updating identity database...");

        await identityContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        logger.LogInformation("identity database Updated");
    }
}
