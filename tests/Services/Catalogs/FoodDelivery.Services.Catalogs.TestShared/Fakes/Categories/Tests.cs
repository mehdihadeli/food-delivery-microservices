using FluentAssertions;
using FoodDelivery.Services.Catalogs.Categories.Data;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Categories;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void CategoryFaker()
    {
        var categories = new CategoryFaker().Generate(5);
        categories.All(x => x.Id > 0).Should().BeTrue();
        categories.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
