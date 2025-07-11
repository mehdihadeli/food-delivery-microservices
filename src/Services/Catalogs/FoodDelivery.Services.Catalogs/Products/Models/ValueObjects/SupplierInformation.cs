using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers;

namespace FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record SupplierInformation
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private SupplierInformation() { }

    public Name Name { get; private set; } = default!;
    public SupplierId Id { get; private set; } = default!;

    public static SupplierInformation Of(SupplierId id, Name name)
    {
        // validations should be placed here instead of constructor
        id.NotBeNull();
        name.NotBeNull();

        return new SupplierInformation { Id = id, Name = name };
    }

    public void Deconstruct(out Name name, out SupplierId supplierId) => (name, supplierId) = (Name, Id);
}
