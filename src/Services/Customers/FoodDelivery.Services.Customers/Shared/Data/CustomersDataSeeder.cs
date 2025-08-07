using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersDataSeeder : IDataSeeder<CustomersDbContext>
{
    public Task SeedAsync(CustomersDbContext context)
    {
        return Task.CompletedTask;
    }
}
