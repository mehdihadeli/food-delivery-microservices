using ECommerce.Services.Catalogs.Suppliers.Data;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Catalogs.TestShared.Fakes.Suppliers;

public class Tests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void SupplierFaker()
    {
        var suppliers = new SupplierDataSeeder.SupplierSeedFaker().Generate(5);
        suppliers.All(x => x.Id > 0).Should().BeTrue();
        suppliers.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
