using Ardalis.GuardClauses;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace ECommerce.Services.Catalogs.Products.ValueObjects;

public record Size
{
    private Size(string value)
    {
        Value = value;
    }

    // Note: in entities with none default constructor, for setting constructor parameter, we need a private set property
    // when we didn't define this property in fluent configuration mapping (if so we can remove private set) , because for getting mapping list of properties to set
    // in the constructor it should not be read only without set (for bypassing calculate fields)- https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
    public string Value { get; private set; }

    public static Size Of(string value)
    {
        // validations should be placed here instead of constructor
        Guard.Against.NullOrWhiteSpace(value);
        return new Size(value);
    }

    public static implicit operator string(Size value) => value.Value;
}
