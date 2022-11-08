using BuildingBlocks.Core.Domain;

namespace ECommerce.Services.Catalogs.Suppliers;

public class Supplier : Entity<SupplierId>
{
    public string Name { get; private set; }

    public Supplier(SupplierId id, string name)
    {
        Name = name;
        Id = id;
    }
}
