using ECommerce.Services.Catalogs.Products.Data;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Catalogs.TestShared.Fakes.Products;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void ProductFaker()
    {
        var products = new ProductDataSeeder.ProductSeedFaker().Generate(5);
        products.All(x => x.Id > 0).Should().BeTrue();
        products.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
