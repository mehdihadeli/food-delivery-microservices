using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Services.Orders.Shared.Data;

public class OrdersDbContextDesignFactory : DbContextDesignFactoryBase<OrdersDbContext>
{
    public OrdersDbContextDesignFactory() : base("OrdersServiceConnection")
    {
    }
}
