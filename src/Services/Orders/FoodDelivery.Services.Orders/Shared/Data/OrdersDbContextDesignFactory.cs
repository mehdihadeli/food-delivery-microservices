using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Services.Orders.Shared.Data;

public class OrdersDbContextDesignFactory : DbContextDesignFactoryBase<OrdersDbContext>
{
    public OrdersDbContextDesignFactory()
        : base("PostgresOptions:ConnectionString") { }
}
