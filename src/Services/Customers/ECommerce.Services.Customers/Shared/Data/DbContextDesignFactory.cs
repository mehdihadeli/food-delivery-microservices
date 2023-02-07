using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Services.Customers.Shared.Data;

public class CustomerDbContextDesignFactory : DbContextDesignFactoryBase<CustomersDbContext>
{
    public CustomerDbContextDesignFactory() : base("PostgresOptions:ConnectionString")
    {
    }
}
