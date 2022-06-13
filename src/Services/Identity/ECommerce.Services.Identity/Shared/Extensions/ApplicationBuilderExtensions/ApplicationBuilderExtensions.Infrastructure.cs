using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;

namespace ECommerce.Services.Identity.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task UseInfrastructure(this IApplicationBuilder app, ILogger logger)
    {
        await app.UsePostgresPersistenceMessage(logger);
    }
}
