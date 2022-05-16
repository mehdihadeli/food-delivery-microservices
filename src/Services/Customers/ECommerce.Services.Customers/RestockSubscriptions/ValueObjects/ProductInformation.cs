using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Domain;

namespace ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;

// Here versioning Name is not important for us so we can save it on DB
public record ProductInformation
{
    public string Name { get; private set; } = null!;
    public ProductId Id { get; private set; } = null!;

    public static ProductInformation Create(ProductId id, string name)
    {
        return new ProductInformation
        {
            Name = Guard.Against.NullOrWhiteSpace(
                name,
                new RestockSubscriptionDomainException("Product name can't be null.")),
            Id = Guard.Against.Null(id, new RestockSubscriptionDomainException("Product Id can't be  null.")),
        };
    }
}
