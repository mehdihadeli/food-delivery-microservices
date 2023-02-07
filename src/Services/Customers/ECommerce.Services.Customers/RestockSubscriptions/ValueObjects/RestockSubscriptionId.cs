using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;

namespace ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;

public record RestockSubscriptionId : AggregateId<long>
{
    // EF
    private RestockSubscriptionId(long value) : base(value)
    {
    }

    public static implicit operator long(RestockSubscriptionId id) => id.Value;

    // validations should be placed here instead of constructor
    public static RestockSubscriptionId Of(long id) => new(Guard.Against.NegativeOrZero(id));
}
