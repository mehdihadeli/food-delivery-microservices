using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Services.Catalogs.Shared.Data;

public class CatalogDbContextDesignFactory : DbContextDesignFactoryBase<CatalogDbContext>
{
    public CatalogDbContextDesignFactory() : base("PostgresOptions:ConnectionString")
    {
    }
}
