using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrdersDataSeeder : IDataSeeder<OrdersDbContext>
{
    public Task SeedAsync(OrdersDbContext context)
    {
        return Task.CompletedTask;
    }
}
