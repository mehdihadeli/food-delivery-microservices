using ECommerce.Services.Catalogs.Categories.Data;
using FluentAssertions;

namespace Tests.Shared.Fakes.Catalogs.Categories;

public class Tests
{
    [Fact]
    public void CategoryFaker()
    {
        var categories = new CategoryDataSeeder.CategoryFaker().Generate(5);
        categories.All(x => x.Id > 0).Should().BeTrue();
        categories.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
