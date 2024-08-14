using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersDataSeeder : IDataSeeder
{
    public int Order => 2;

    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
