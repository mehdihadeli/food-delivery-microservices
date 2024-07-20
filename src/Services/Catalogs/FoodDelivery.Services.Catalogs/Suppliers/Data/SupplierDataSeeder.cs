using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Suppliers.Data;

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

        var suppliers = new SupplierFaker().Generate(5);
        await _dbContext.Suppliers.AddRangeAsync(suppliers);

        await _dbContext.SaveChangesAsync();
    }

    public int Order => 2;
}

// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with CustomInstantiator
