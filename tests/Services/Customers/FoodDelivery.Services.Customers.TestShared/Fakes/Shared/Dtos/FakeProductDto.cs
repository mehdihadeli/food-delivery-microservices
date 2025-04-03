using AutoBogus;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Dtos;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Dtos;

//https://github.com/bchavez/Bogus#the-great-c-example
//https://github.com/bchavez/Bogus#bogus-api-support
//https://github.com/nickdodd79/AutoBogus/issues/99
public sealed class FakeProductDto : AutoFaker<ProductClientDto>
{
    public FakeProductDto()
    {
        long id = 1;
        RuleFor(x => x.ProductStatus, f => f.PickRandom<ProductStatus>())
            //https://github.com/nickdodd79/AutoBogus/issues/99
            .RuleForType(typeof(int), faker => faker.Random.Int(min: 1, max: int.MaxValue))
            .RuleForType(typeof(long), faker => faker.Random.Long(min: 1, max: long.MaxValue))
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
            .RuleFor(x => x.Id, f => id++)
            .RuleFor(x => x.BrandId, f => f.Random.Number(1, 100))
            .RuleFor(x => x.CategoryId, f => f.Random.Number(1, 100))
            .RuleFor(x => x.SupplierId, f => f.Random.Number(1, 100))
            .RuleFor(x => x.BrandName, f => f.Company.CompanyName())
            .RuleFor(x => x.CategoryName, f => f.Commerce.Categories(1).First())
            .RuleFor(x => x.SupplierName, f => f.Name.FullName())
            .RuleFor(x => x.Price, f => decimal.Parse(f.Commerce.Price()))
            .RuleFor(x => x.AvailableStock, f => f.Random.Number(10, 100))
            .RuleFor(x => x.RestockThreshold, f => f.Random.Number(5))
            .RuleFor(x => x.MaxStockThreshold, f => f.Random.Number(100));
    }
}
