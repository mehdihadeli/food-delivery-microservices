using Bogus;
using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FoodDelivery.Services.Catalogs.Products.Data;

public class ProductDataSeeder(ICatalogDbContext dbContext) : IDataSeeder
{
    public async Task SeedAllAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken: cancellationToken))
            return;

        long id = 1;

        var supplierChecker = Substitute.For<ISupplierChecker>();
        supplierChecker.SupplierExists(Arg.Any<SupplierId>()).Returns(true);

        var categoryChecker = Substitute.For<ICategoryChecker>();
        categoryChecker.CategoryExists(Arg.Any<CategoryId>()).Returns(true);

        var brandChecker = Substitute.For<IBrandChecker>();
        brandChecker.BrandExists(Arg.Any<BrandId>()).Returns(true);

        var categoryIds = dbContext.Categories.Select(x => x.Id).ToList();
        var brandIds = dbContext.Brands.Select(x => x.Id).ToList();
        var supplierIds = dbContext.Suppliers.Select(x => x.Id).ToList();

        // we should not instantiate customer aggregate manually because it is possible we break aggregate invariant in creating a product, and it is better we
        // create a product with its factory method
        var productFaker = new Faker<Product>().CustomInstantiator(faker =>
        {
            var product = Product.Create(
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
                faker.PickRandom<CategoryId>(categoryIds),
                faker.PickRandom<SupplierId>(supplierIds),
                faker.PickRandom<BrandId>(brandIds),
                categoryChecker,
                supplierChecker,
                brandChecker
            );

            return product;
        });
        var products = productFaker.Generate(5);

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public int Order => 4;
}

// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one
