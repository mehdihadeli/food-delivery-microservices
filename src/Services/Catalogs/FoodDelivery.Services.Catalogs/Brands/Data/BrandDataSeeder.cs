using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Brands.Data;

public class BrandDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _context;

    public BrandDataSeeder(ICatalogDbContext context)
    {
        _context = context;
    }

    public async Task SeedAllAsync()
    {
        if (await _context.Brands.AnyAsync())
            return;

        // https://www.youtube.com/watch?v=T9pwE1GAr_U
        // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
        // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
        // https://github.com/bchavez/Bogus#bogus-api-support
        // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74

        // faker works with normal syntax because brand has a default constructor
        var brands = new BrandFaker().Generate(5);

        await _context.Brands.AddRangeAsync(brands);
        await _context.SaveChangesAsync();
    }

    public int Order => 3;
}

// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one
