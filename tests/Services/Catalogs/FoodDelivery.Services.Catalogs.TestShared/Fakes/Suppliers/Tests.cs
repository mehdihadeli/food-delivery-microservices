using FluentAssertions;
using FoodDelivery.Services.Catalogs.Suppliers.Data;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Catalogs.TestShared.Fakes.Suppliers;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void SupplierFaker()
    {
        var suppliers = new SupplierFaker().Generate(5);
        suppliers.All(x => x.Id > 0).Should().BeTrue();
        suppliers.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
