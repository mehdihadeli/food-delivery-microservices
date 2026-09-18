using CloudNativeKit.Persistence.EfCore.Postgres;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomerDbContextDesignFactory()
    : DbContextDesignFactoryBase<CustomersDbContext>("PostgresOptions:ConnectionString");
