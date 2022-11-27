using AutoBogus;
using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

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

        // https://www.youtube.com/watch?v=T9pwE1GAr_U
        // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
        // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
        // https://github.com/bchavez/Bogus#bogus-api-support
        // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74
        long id = 1;
        var suppliersFaker = new AutoFaker<Supplier>()

            // Call for objects that have complex initialization
            // faker doesn't work with normal syntax because it has no default constructor
            .CustomInstantiator(faker =>
            {
                var supplier = new Supplier(id++, faker.Person.FullName);
                return supplier;
            }).FinishWith((f, u) =>
            {
                Console.WriteLine("Supplier Created! Id={0}", u.Id);
            });

        var suppliers = suppliersFaker.Generate(5);
        await _dbContext.Suppliers.AddRangeAsync(suppliers);

        await _dbContext.SaveChangesAsync();
    }

    public int Order => 2;
}
