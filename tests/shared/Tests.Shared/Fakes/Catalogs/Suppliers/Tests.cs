using ECommerce.Services.Catalogs.Suppliers.Data;
using FluentAssertions;

namespace Tests.Shared.Fakes.Catalogs.Suppliers;

public class Tests
{
    [Fact]
    public void SupplierFaker()
    {
        var suppliers = new SupplierDataSeeder.SupplierFaker().Generate(5);
        suppliers.All(x => x.Id > 0).Should().BeTrue();
        suppliers.All(x => string.IsNullOrWhiteSpace(x.Name)).Should().BeFalse();
    }
}
