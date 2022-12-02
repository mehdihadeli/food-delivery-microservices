using ECommerce.Services.Catalogs.Products.Data;
using FluentAssertions;

namespace Tests.Shared.Fakes.Catalogs.Products;

public class Tests
{
    [Fact]
    public void ProductFaker()
    {
        var products = new ProductDataSeeder.ProductFaker().Generate(5);
        products.All(x => x.Id > 0).Should().BeTrue();
        products.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
