using ECommerce.Services.Customers.Customers.Exceptions.Domain;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace ECommerce.Services.Customers.Customers.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Nationality
{
    private static readonly HashSet<string> _allowedNationality = new()
    {
        "IR",
        "DE",
        "FR",
        "ES",
        "GB",
        "US"
    };

    // EF
    public Nationality(string value)
    {
        Value = value;
    }

    // Note: in entities with none default constructor, for setting constructor parameter, we need a private set property
    // when we didn't define this property in fluent configuration map, because for getting mapping list of properties to set
    // in the constructor it should not be read only without set (for bypassing calculate fields)- https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
    public string Value { get; private set; } = default!;

    public static Nationality Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 2)
        {
            throw new InvalidNationalityException(value);
        }

        value = value.ToUpperInvariant();
        if (!_allowedNationality.Contains(value))
        {
            throw new UnsupportedNationalityException(value);
        }

        return new Nationality(value);
    }

    public static implicit operator string(Nationality value) => value.Value;
}
