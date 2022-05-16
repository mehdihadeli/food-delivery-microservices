using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Orders.Orders.ValueObjects;

public record OrderId : AggregateId
{
    public OrderId(long value) : base(value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
    }

    public static implicit operator long(OrderId id) => Guard.Against.Null(id.Value, nameof(id.Value));

    public static implicit operator OrderId(long id) => new(id);
}
