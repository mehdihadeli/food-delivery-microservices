using Bogus;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Categories.ValueObjects;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FoodDelivery.Services.Catalogs.Shared.Data;

public class CatalogsDataSeeder : IDataSeeder<CatalogDbContext>
{
    public async Task SeedAsync(CatalogDbContext context)
    {
        await SeedCategories(context);
        await SeedSuppliers(context);
        await SeedBrands(context);
        await SeedProducts(context);
    }

    private static async Task SeedCategories(CatalogDbContext context)
    {
        if (await context.Categories.AnyAsync())
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

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSuppliers(CatalogDbContext dbContext)
    {
        if (await dbContext.Suppliers.AnyAsync())
            return;

        var id = 1;
        var supplierFaker = new Faker<Supplier>().CustomInstantiator(faker =>
        {
            var supplier = new Supplier(SupplierId.Of(id++), faker.Person.FullName);
            return supplier;
        });

        var suppliers = supplierFaker.Generate(5);
        await dbContext.Suppliers.AddRangeAsync(suppliers);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedBrands(CatalogDbContext context)
    {
        if (await context.Brands.AnyAsync())
            return;

        long id = 1;
        var brandFaker = new Faker<Brand>().CustomInstantiator(f =>
            Brand.Create(BrandId.Of(id++), BrandName.Of(f.Company.CompanyName()))
        );
        var brands = brandFaker.Generate(5);

        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProducts(CatalogDbContext dbContext)
    {
        if (await dbContext.Products.AnyAsync())
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

        await dbContext.Products.AddRangeAsync(products);
        await dbContext.SaveChangesAsync();
    }
}
