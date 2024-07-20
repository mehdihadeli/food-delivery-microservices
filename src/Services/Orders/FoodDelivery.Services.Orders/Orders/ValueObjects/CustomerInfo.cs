using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Orders.Orders.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record CustomerInfo
{
    // EF
    private CustomerInfo() { }

    public string Name { get; private set; } = default!;
    public long CustomerId { get; private set; }

    public static CustomerInfo Of([NotNull] string? name, long customerId)
    {
        // validations should be placed here instead of constructor
        name.NotBeEmptyOrNull();
        customerId.NotBeNegativeOrZero();

        return new CustomerInfo { Name = name, CustomerId = customerId };
    }

    public void Deconstruct(out string name, out long customerId) => (name, customerId) = (Name, CustomerId);
}
