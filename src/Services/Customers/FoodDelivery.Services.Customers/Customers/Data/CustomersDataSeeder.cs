using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Customers.Customers.Data;

public class CustomersDataSeeder : IDataSeeder
{
    public Task SeedAllAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public int Order => 1;
}
