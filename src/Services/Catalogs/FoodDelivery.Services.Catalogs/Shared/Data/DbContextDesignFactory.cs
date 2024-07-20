using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogDbContextDesignFactory : DbContextDesignFactoryBase<CatalogDbContext>
{
    public CatalogDbContextDesignFactory()
        : base("PostgresOptions:ConnectionString") { }
}
