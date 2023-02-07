using Ardalis.GuardClauses;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Price
{
    // EF
    private Price(decimal value)
    {
        Value = value;
    }

    // Note: in entities with none default constructor, for setting constructor parameter, we need a private set property
    // when we didn't define this property in fluent configuration mapping (if so we can remove private set) , because for getting mapping list of properties to set
    // in the constructor it should not be read only without set (for bypassing calculate fields)- https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
    public decimal Value { get; private set; }

    public static Price Of(decimal value)
    {
        // validations should be placed here instead of constructor
        Guard.Against.NegativeOrZero(value);

        return new Price(value);
    }

    public static implicit operator decimal(Price value) => value.Value;
}
