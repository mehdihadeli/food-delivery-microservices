using ECommerce.Services.Catalogs.Categories.Data;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Catalogs.TestShared.Fakes.Categories;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void CategoryFaker()
    {
        var categories = new CategoryDataSeeder.CategorySeedFaker().Generate(5);
        categories.All(x => x.Id > 0).Should().BeTrue();
        categories.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
