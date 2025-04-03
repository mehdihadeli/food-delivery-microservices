using Bogus;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Categories.ValueObjects;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Categories.Data;

public class CategoryDataSeeder(ICatalogDbContext dbContext) : IDataSeeder
{
    public async Task SeedAllAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Categories.AnyAsync(cancellationToken: cancellationToken))
            return;

        var categoryId = 1;
        var imageId = 1;
        var categoryFaker = new Faker<Category>().CustomInstantiator(f =>
        {
            var generatedCid = CategoryId.Of(categoryId++);
            var generatedImageId = EntityId.Of(imageId++);

            var category = Category.Create(
                generatedCid,
                CategoryName.Of(f.Commerce.Categories(1).First()),
                CategoryCode.Of(f.Random.Number(1000, 5000).ToString()),
                new CategoryImage(generatedImageId, f.Internet.Url(), f.Random.Bool(), generatedCid),
                Description.Of(f.Commerce.ProductDescription())
            );

            return category;
        });
        var categories = categoryFaker.Generate(5);

        await dbContext.Categories.AddRangeAsync(categories, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public int Order => 1;
}
