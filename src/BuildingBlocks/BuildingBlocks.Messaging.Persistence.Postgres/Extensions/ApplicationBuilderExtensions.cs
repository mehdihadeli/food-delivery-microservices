using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.Persistence.Postgres.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task UsePostgresPersistenceMessage(this IApplicationBuilder app, ILogger logger)
    {
        await ApplyDatabaseMigrations(app, logger);
    }

    private static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        var messagePersistenceDbContext = serviceScope.ServiceProvider.GetRequiredService<MessagePersistenceDbContext>();

        logger.LogInformation("Applying persistence-message migrations...");

        await messagePersistenceDbContext.Database.MigrateAsync();

        logger.LogInformation("persistence-message migrations applied.");
    }
}
