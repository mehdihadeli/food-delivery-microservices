using ECommerce.Services.Catalogs.Brands.Data;
using FluentAssertions;

namespace Tests.Shared.Fakes.Catalogs.Banners;

public class Tests
{
    [Fact]
    public void BrandFaker()
    {
        var banners = new BrandDataSeeder.BrandFaker().Generate(5);
        banners.All(x => x.Id > 0).Should().BeTrue();
        banners.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
