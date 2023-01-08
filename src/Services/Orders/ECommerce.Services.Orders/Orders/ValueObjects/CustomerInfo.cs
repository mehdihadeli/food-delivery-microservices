using Ardalis.GuardClauses;

namespace ECommerce.Services.Orders.Orders.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record CustomerInfo
{
    // EF
    public CustomerInfo()
    {
    }

    public string Name { get; private set; } = default!;
    public long CustomerId { get; private set; }

    public static CustomerInfo Of(string name, long customerId)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NegativeOrZero(customerId);

        return new CustomerInfo {Name = name, CustomerId = customerId};
    }
}
