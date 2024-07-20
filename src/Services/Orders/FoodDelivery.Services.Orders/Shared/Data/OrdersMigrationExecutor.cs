using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrdersMigrationExecutor : IMigrationExecutor
{
    private readonly OrdersDbContext _ordersDbContext;
    private readonly ILogger<OrdersMigrationExecutor> _logger;

    public OrdersMigrationExecutor(OrdersDbContext ordersDbContext, ILogger<OrdersMigrationExecutor> logger)
    {
        _ordersDbContext = ordersDbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration worker started");

        _logger.LogInformation("Updating identity database...");

        await _ordersDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("identity database Updated");
    }
}
