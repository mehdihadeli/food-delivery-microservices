using BuildingBlocks.Persistence.EfCore.Postgres;

namespace Store.Services.Customers.Shared.Data;

public class CatalogDbContextDesignFactory : DbContextDesignFactoryBase<CustomersDbContext>
{
    public CatalogDbContextDesignFactory() : base("CustomersServiceConnection")
    {
    }
}
