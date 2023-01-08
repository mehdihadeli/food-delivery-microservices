using Ardalis.GuardClauses;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Name
{
    // EF
    public Name()
    {
    }

    public string Value { get; private set; } = default!;

    public static Name Of(string value)
    {
        // validations should be placed here instead of constructor
        Guard.Against.NullOrEmpty(value);

        return new Name {Value = value};
    }

    public static implicit operator string(Name value) => value.Value;
}
