using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;

public class MigrationSchema(MessagePersistenceDbContext messagePersistenceDbContext, ILogger<MigrationSchema> logger)
    : IMigrationSchema
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Applying persistence-message migrations...");

        await messagePersistenceDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        logger.LogInformation("persistence-message migrations applied");
    }
}
