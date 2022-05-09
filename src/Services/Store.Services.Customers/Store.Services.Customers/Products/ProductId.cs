using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace Store.Services.Customers.Products;

public record ProductId : AggregateId<long>
{
    public ProductId(long value) : base(value)
    {
    }

    public static implicit operator long(ProductId id) => Guard.Against.Null(id.Value, nameof(id.Value));

    public static implicit operator ProductId(long id) => new(id);
}
