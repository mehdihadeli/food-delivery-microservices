using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record SupplierInformation
{
    // EF
    private SupplierInformation()
    {
    }

    public Name Name { get; private set; } = default!;
    public SupplierId Id { get; private set; } = default!;

    public static SupplierInformation Of(SupplierId id, Name name)
    {
        // validations should be placed here instead of constructor
        Guard.Against.Null(id, new ProductDomainException("SupplierId can not be null."));
        Guard.Against.Null(name, new ProductDomainException("Name cannot be null."));

        return new SupplierInformation {Id = id, Name = name};
    }
}
