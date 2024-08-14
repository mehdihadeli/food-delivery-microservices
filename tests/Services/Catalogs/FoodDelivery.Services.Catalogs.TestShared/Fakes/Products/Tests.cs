using FluentAssertions;
using FoodDelivery.Services.Catalogs.Products.Data;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Products;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void ProductFaker()
    {
        var products = new ProductFaker().Generate(5);
        products.All(x => x.Id > 0).Should().BeTrue();
        products.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
