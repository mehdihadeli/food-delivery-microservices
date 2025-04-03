using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrdersDataSeeder : IDataSeeder
{
    public Task SeedAllAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public int Order => 1;
}
