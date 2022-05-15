using BuildingBlocks.Persistence.EfCore.Postgres;

namespace Store.Services.Customers.Shared.Data;

public class CustomerDbContextDesignFactory : DbContextDesignFactoryBase<CustomersDbContext>
{
    public CustomerDbContextDesignFactory() : base("CustomersServiceConnection")
    {
    }
}
