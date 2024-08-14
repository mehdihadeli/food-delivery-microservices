using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Services.Identity.Shared.Data;

public class DbContextDesignFactory() : DbContextDesignFactoryBase<IdentityContext>("PostgresOptions:ConnectionString");
