using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CustomersMigrationExecutor : IMigrationExecutor
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<CustomersMigrationExecutor> _logger;

    public CustomersMigrationExecutor(CustomersDbContext customersDbContext, ILogger<CustomersMigrationExecutor> logger)
    {
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Migration worker started");

        _logger.LogInformation("Updating customers database...");

        await _customersDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("customers database Updated");
    }
}
