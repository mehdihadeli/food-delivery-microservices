using AutoBogus;
using ECommerce.Services.Customers.Shared.Clients.Catalogs.Dtos;

namespace ECommerce.Services.Customers.TestShared.Fakes.Shared.Dtos;

//https://github.com/bchavez/Bogus#the-great-c-example
//https://github.com/bchavez/Bogus#bogus-api-support
public sealed class FakeProductDto : AutoFaker<ProductDto>
{
    public FakeProductDto()
    {
        long id = 1;
        RuleFor(x => x.ProductStatus, f => f.PickRandom<ProductStatus>())
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
