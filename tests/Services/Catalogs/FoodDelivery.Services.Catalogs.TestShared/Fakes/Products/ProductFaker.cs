using Bogus;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using NSubstitute;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Products;

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

        var supplierChecker = Substitute.For<ISupplierChecker>();
        supplierChecker.SupplierExists(Arg.Any<SupplierId>()).Returns(true);

        var categoryChecker = Substitute.For<ICategoryChecker>();
        categoryChecker.CategoryExists(Arg.Any<CategoryId>()).Returns(true);

        var brandChecker = Substitute.For<IBrandChecker>();
        brandChecker.BrandExists(Arg.Any<BrandId>()).Returns(true);

        // we should not instantiate customer aggregate manually because it is possible we break aggregate invariant in creating a product, and it is better we
        // create a product with its factory method
        CustomInstantiator(faker =>
            Product.Create(
                ProductId.Of(id++),
                Name.Of(faker.Commerce.ProductName()),
                ProductInformation.Of(faker.Commerce.ProductName(), faker.Commerce.ProductDescription()),
                Stock.Of(faker.Random.Int(10, 20), 5, 20),
                faker.PickRandom<ProductStatus>(),
                faker.PickRandom<ProductType>(),
                Dimensions.Of(faker.Random.Int(10, 50), faker.Random.Int(10, 50), faker.Random.Int(10, 50)),
                Size.Of(faker.PickRandom<string>("M", "S", "L")),
                faker.Random.Enum<ProductColor>(),
                faker.Commerce.ProductDescription(),
                Price.Of(faker.PickRandom<decimal>(100, 200, 500)),
                CategoryId.Of(faker.Random.Long(1, 3)),
                SupplierId.Of(faker.Random.Long(1, 5)),
                BrandId.Of(faker.Random.Long(1, 5)),
                categoryChecker,
                supplierChecker,
                brandChecker
            )
        );
    }
}
