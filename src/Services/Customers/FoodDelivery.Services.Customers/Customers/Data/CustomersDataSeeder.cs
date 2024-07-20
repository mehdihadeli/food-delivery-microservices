using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Customers.Customers.Data;

public class CustomersDataSeeder : IDataSeeder
{
    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }

    public int Order => 1;
}
