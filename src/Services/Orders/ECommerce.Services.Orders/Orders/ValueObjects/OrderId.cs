using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Orders.Orders.ValueObjects;

public record OrderId : AggregateId
{
    // EF
    protected OrderId(long value) : base(value)
    {
    }

    public static implicit operator long(OrderId id) => id.Value;

    // validations should be placed here instead of constructor
    public static OrderId Of(long id) => new(Guard.Against.NegativeOrZero(id));
}
