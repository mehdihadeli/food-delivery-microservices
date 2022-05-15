using BuildingBlocks.Persistence.EfCore.Postgres;

namespace Store.Services.Orders.Shared.Data;

public class OrdersDbContextDesignFactory : DbContextDesignFactoryBase<OrdersDbContext>
{
    public OrdersDbContextDesignFactory() : base("OrdersServiceConnection")
    {
    }
}
