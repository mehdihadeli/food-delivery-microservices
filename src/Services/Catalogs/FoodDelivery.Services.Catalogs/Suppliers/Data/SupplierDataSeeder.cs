using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Suppliers.Data;

public class SupplierDataSeeder(ICatalogDbContext dbContext) : IDataSeeder
{
    public async Task SeedAllAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Suppliers.AnyAsync(cancellationToken: cancellationToken))
            return;

        var id = 1;
        var supplierFaker = new Faker<Supplier>().CustomInstantiator(faker =>
        {
            var supplier = new Supplier(SupplierId.Of(id++), faker.Person.FullName);
            return supplier;
        });

        var suppliers = supplierFaker.Generate(5);
        await dbContext.Suppliers.AddRangeAsync(suppliers, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public int Order => 2;
}

// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with CustomInstantiator
