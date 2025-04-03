using FluentAssertions;
using Tests.Shared.XunitCategories;
using Xunit;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Banners;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void BrandFaker()
    {
        var banners = new BrandFaker().Generate(5);
        banners.All(x => x.Id > 0).Should().BeTrue();
        banners.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
