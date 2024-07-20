using System.Globalization;
using Bogus;
using BuildingBlocks.Abstractions.Domain;
using FoodDelivery.Services.Catalogs.Categories.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Categories.Data;

public sealed class CategoryFaker : Faker<Category>
{
    public CategoryFaker()
    {
        var categoryId = 1;
        var imageId = 1;

        CustomInstantiator(f =>
        {
            var generatedCid = CategoryId.Of(categoryId++);
            return Category.Create(
                generatedCid,
               CategoryName.Of(f.Commerce.Categories(1).First()),
               CategoryCode.Of(f.Random.Number(1000, 5000).ToString()),
                new CategoryImage(EntityId.Of(imageId), f.Internet.Url(), f.Random.Bool(), generatedCid),
                 f.Commerce.ProductDescription());
        });
    }
}
