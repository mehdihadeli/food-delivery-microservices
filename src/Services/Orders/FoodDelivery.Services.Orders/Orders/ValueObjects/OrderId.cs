using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Orders.Orders.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public record OrderId : AggregateId
{
    // EF
    private OrderId(long value)
        : base(value) { }

    // validations should be placed here instead of constructor
    public static OrderId Of(long id) => new(id.NotBeNegativeOrZero());

    public static implicit operator long(OrderId id) => id.Value;
}
