using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Products.Data;

public class ProductDataSeeder : IDataSeeder
{
    // because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with CustomInstantiator
    public sealed class ProductFaker : Faker<Product>
    {
        public ProductFaker()
        {
            // https://www.youtube.com/watch?v=T9pwE1GAr_U
            // https://jackhiston.com/2017/10/1/how-to-create-bogus-data-in-c/
            // https://khalidabuhakmeh.com/seed-entity-framework-core-with-bogus
            // https://github.com/bchavez/Bogus#bogus-api-support
            // https://github.com/bchavez/Bogus/blob/master/Examples/EFCoreSeedDb/Program.cs#L74
            long id = 1;

            CustomInstantiator(faker => Product.Create(
                id++,
                faker.Commerce.ProductName(),
                Stock.Create(faker.Random.Int(10, 20), 5, 20),
                faker.PickRandom<ProductStatus>(),
                Dimensions.Create(faker.Random.Int(10, 50), faker.Random.Int(10, 50), faker.Random.Int(10, 50)),
                faker.PickRandom<string>("M", "S", "L"),
                faker.Random.Enum<ProductColor>(),
                faker.Commerce.ProductDescription(),
                Price.Create(faker.PickRandom<decimal>(100, 200, 500)),
                faker.Random.Long(1, 3),
                faker.Random.Long(1, 5),
                faker.Random.Long(1, 5)));
        }
    }

    private readonly ICatalogDbContext _dbContext;

    public ProductDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Products.AnyAsync())
            return;

        var products = new ProductFaker().Generate(5);

        await _dbContext.Products.AddRangeAsync(products);
        await _dbContext.SaveChangesAsync();
    }

    public int Order => 4;
}
