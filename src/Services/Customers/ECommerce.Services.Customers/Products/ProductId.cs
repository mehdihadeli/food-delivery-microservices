using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Customers.Products;

public record ProductId : AggregateId<long>
{
    // EF
    protected ProductId(long value) : base(value)
    {
    }

    public static implicit operator long(ProductId id) => id.Value;

    // validations should be placed here instead of constructor
    public static ProductId Of(long id) => new(Guard.Against.NegativeOrZero(id));
}
