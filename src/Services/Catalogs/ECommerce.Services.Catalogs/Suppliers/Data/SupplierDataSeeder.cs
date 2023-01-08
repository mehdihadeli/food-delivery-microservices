using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Suppliers.Data;

public class SupplierDataSeeder : IDataSeeder
{
    // because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with CustomInstantiator
    public sealed class SupplierSeedFaker : Faker<Supplier>
    {
        public SupplierSeedFaker()
        {
            long id = 1;

            // https://www.youtube.com/watch?v=T9pwE1GAr_U
            // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
            // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
            // https://github.com/bchavez/Bogus#bogus-api-support
            // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74

            // Call for objects that have complex initialization
            // faker doesn't work with normal syntax because it has no default constructor
            CustomInstantiator(faker =>
            {
                var supplier = new Supplier(SupplierId.Of(id++), faker.Person.FullName);
                return supplier;
            });
        }
    }

    private readonly ICatalogDbContext _dbContext;

    public SupplierDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Suppliers.AnyAsync())
            return;

        var suppliers = new SupplierSeedFaker().Generate(5);
        await _dbContext.Suppliers.AddRangeAsync(suppliers);

        await _dbContext.SaveChangesAsync();
    }

    public int Order => 2;
}
