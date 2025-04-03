using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrdersMigrationSchema(OrdersDbContext ordersDbContext, ILogger<OrdersMigrationSchema> logger)
    : IMigrationSchema
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migration worker started");

        logger.LogInformation("Updating identity database...");

        await ordersDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        logger.LogInformation("identity database Updated");
    }
}
