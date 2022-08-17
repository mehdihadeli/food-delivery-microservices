using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Shared.Contracts;

namespace ECommerce.Services.Catalogs.Suppliers.Data;

public class SupplierDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _dbContext;

    public SupplierDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Suppliers.AnyAsync())
            return;

        long id = 1;
        var suppliersFaker = new Faker<Supplier>().CustomInstantiator(faker =>
        {
            var supplier = new Supplier(id, faker.Person.FullName);
            id++;
            return supplier;
        });

        var suppliers = suppliersFaker.Generate(5);
        await _dbContext.Suppliers.AddRangeAsync(suppliers);

        await _dbContext.SaveChangesAsync();
    }

    public int Order => 2;
}
