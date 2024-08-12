using Bogus;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.Data;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Data;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers.Data;
using NSubstitute;

namespace FoodDelivery.Services.Catalogs.IntegrationTests;

public class UnitTest1
{
    [Fact]
    [Trait("Category", "Integration")]
    public void Test1()
    {
        long id = 1;
        var category = new CategoryFaker().Generate();
        var supplier = new SupplierFaker().Generate();
        var brand = new BrandFaker().Generate();

        var supplierChecker = Substitute.For<ISupplierChecker>();
        supplierChecker.SupplierExists(Arg.Any<SupplierId>()).Returns(true);

        var brandChecker = Substitute.For<IBrandChecker>();
        brandChecker.BrandExists(Arg.Any<BrandId>()).Returns(true);

        // Call for objects that have complex initialization
        var productFaker = new Faker<Product>().CustomInstantiator(faker =>
            Product.Create(
                ProductId.Of(id++),
                Name.Of(faker.Commerce.ProductName()),
                ProductInformation.Of(faker.Commerce.ProductName(), faker.Commerce.ProductDescription()),
                Stock.Of(faker.Random.Int(10, 20), 5, 20),
                ProductStatus.Available,
                faker.Random.Enum<ProductType>(),
                Dimensions.Of(faker.Random.Int(10, 50), faker.Random.Int(10, 50), faker.Random.Int(10, 50)),
                Size.Of(faker.PickRandom<string>("M", "S", "L")),
                faker.Random.Enum<ProductColor>(),
                faker.Commerce.ProductDescription(),
                Price.Of(faker.PickRandom<decimal>(100, 200, 500)),
                category.Id,
                SupplierId.Of(faker.Random.Long(1, 5)),
                BrandId.Of(faker.Random.Long(1, 5)),
                _ => Task.FromResult(true)!,
                supplierChecker,
                brandChecker
            )
        );

        var s = productFaker.Generate(5);
    }
}
