using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Customers.Customers.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record CustomerId : AggregateId
{
    // EF
    private CustomerId(long value) : base(value)
    {
    }

    // validations should be placed here instead of constructor
    public static CustomerId Of(long id) => new(Guard.Against.NegativeOrZero(id));

    public static implicit operator long(CustomerId id) => id.Value;
}
