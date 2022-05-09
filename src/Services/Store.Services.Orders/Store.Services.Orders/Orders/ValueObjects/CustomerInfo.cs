using BuildingBlocks.Core.Domain;

namespace Store.Services.Orders.Orders.ValueObjects;

public class CustomerInfo : ValueObject
{
    public string Name { get; private set; }
    public long CustomerId { get; private set; }

    public static CustomerInfo Create(string name, long customerId)
    {
        return new CustomerInfo { Name = name, CustomerId = customerId };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return CustomerId;
    }
}
