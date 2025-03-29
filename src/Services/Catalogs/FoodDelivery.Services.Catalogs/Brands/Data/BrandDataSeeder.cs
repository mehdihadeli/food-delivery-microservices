using AutoBogus;
using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Brands.Data;

public class BrandDataSeeder(ICatalogDbContext context) : IDataSeeder
{
    public async Task SeedAllAsync(CancellationToken cancellationToken)
    {
        if (await context.Brands.AnyAsync(cancellationToken: cancellationToken))
            return;

        long id = 1;
        var brandFaker = new Faker<Brand>().CustomInstantiator(f =>
            Brand.Create(BrandId.Of(id++), BrandName.Of(f.Company.CompanyName()))
        );
        var brands = brandFaker.Generate(5);

        await context.Brands.AddRangeAsync(brands, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public int Order => 3;
}

// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one
