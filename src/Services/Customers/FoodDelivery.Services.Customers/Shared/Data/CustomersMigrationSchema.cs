using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersMigrationSchema(CustomersDbContext customersDbContext, ILogger<CustomersMigrationSchema> logger)
    : IMigrationSchema
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Migration worker started");

        logger.LogInformation("Updating customers database...");

        await customersDbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Customers database Updated");
    }
}
