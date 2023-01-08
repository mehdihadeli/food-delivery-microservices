using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Domain;

namespace ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;

// Here versioning Name is not important for us so we can save it on DB
public record ProductInformation
{
    // EF
    public ProductInformation()
    {
    }

    public string Name { get; private set; } = default!;
    public ProductId Id { get; private set; } = default!;

    public static ProductInformation Of(ProductId id, string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(id);

        return new ProductInformation {Name = name, Id = id};
    }
}
