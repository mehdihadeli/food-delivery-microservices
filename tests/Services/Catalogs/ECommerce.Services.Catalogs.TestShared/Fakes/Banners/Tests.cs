using ECommerce.Services.Catalogs.Brands.Data;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Catalogs.TestShared.Fakes.Banners;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void BrandFaker()
    {
        var banners = new BrandDataSeeder.BrandSeedFaker().Generate(5);
        banners.All(x => x.Id > 0).Should().BeTrue();
        banners.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
