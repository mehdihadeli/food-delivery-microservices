using ECommerce.Services.Customers.Customers.Exceptions;
using ECommerce.Services.Customers.Customers.Exceptions.Domain;

namespace ECommerce.Services.Customers.Customers.ValueObjects;

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

    public string Value { get; private set; }

    public static Nationality? Null => null;

    public static Nationality Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 2)
        {
            throw new InvalidNationalityException(value ?? "null");
        }

        value = value.ToUpperInvariant();
        if (!_allowedNationality.Contains(value))
        {
            throw new UnsupportedNationalityException(value);
        }

        return new Nationality { Value = value };
    }

    public static implicit operator Nationality?(string? value) => value is null ? null : Create(value);

    public static implicit operator string?(Nationality? value) => value?.Value;
}
