using BuildingBlocks.Core.Domain;
using ECommerce.Services.Customers.Customers.Exceptions.Domain;

namespace ECommerce.Services.Customers.Customers.Models;

public class CustomerName : ValueObject
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set;} = null!;
    public string FullName => FirstName + " " + LastName;

    public static readonly CustomerName Empty = new();
    public static readonly CustomerName? Null = null;

    public static CustomerName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || firstName.Length is > 100 or < 3)
        {
            throw new InvalidNameException(firstName ?? "null");
        }

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Length is > 100 or < 3)
        {
            throw new InvalidNameException(lastName ?? "null");
        }

        return new CustomerName
        {
            FirstName = firstName,
            LastName = lastName,
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}
