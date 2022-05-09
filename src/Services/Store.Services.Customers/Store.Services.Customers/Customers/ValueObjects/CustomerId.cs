using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace Store.Services.Customers.Customers.ValueObjects;

public record CustomerId : AggregateId
{
    public CustomerId(long value) : base(value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
    }

    public static implicit operator long(CustomerId id) => Guard.Against.Null(id.Value, nameof(id.Value));

    public static implicit operator CustomerId(long id) => new(id);
}
