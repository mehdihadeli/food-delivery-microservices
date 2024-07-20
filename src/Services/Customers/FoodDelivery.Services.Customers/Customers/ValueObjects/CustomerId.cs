using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Customers.Customers.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record CustomerId : AggregateId
{
    // EF
    private CustomerId(long value)
        : base(value) { }

    // validations should be placed here instead of constructor
    public static CustomerId Of(long id) => new(id.NotBeNegativeOrZero());

    public static implicit operator long(CustomerId id) => id.Value;
}
