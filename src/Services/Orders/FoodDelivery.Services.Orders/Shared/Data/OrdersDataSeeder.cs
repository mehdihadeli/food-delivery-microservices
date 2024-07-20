using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrdersDataSeeder : IDataSeeder
{
    public int Order => 1;

    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
