using BuildingBlocks.Core.Domain;
using ECommerce.Services.Customers.Customers.Exceptions.Domain;

namespace ECommerce.Services.Customers.Customers.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public class CustomerName : ValueObject
{
    // EF
    private CustomerName()
    {
    }

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => FirstName + " " + LastName;

    public static CustomerName Of(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length is > 100 or < 3)
        {
            throw new InvalidNameException(firstName);
        }

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length is > 100 or < 3)
        {
            throw new InvalidNameException(lastName);
        }

        return new CustomerName {FirstName = firstName, LastName = lastName};
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}
