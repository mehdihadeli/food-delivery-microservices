using Bogus;
using BuildingBlocks.Abstractions.Domain;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.ValueObjects;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Categories;

public sealed class CategoryFaker : Faker<Category>
{
    public CategoryFaker()
    {
        var categoryId = 1;
        var imageId = 1;

        CustomInstantiator(f =>
        {
            var generatedCid = CategoryId.Of(categoryId++);
            var generatedImageId = EntityId.Of(imageId++);

            return Category.Create(
                generatedCid,
                CategoryName.Of(f.Commerce.Categories(1).First()),
                CategoryCode.Of(f.Random.Number(1000, 5000).ToString()),
                new CategoryImage(generatedImageId, f.Internet.Url(), f.Random.Bool(), generatedCid),
                Description.Of(f.Commerce.ProductDescription())
            );
        });
    }
}
