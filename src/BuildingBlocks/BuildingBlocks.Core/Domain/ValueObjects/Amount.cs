using BuildingBlocks.Core.Exception.Types;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Amount
{
    // EF
    private Amount(decimal value)
    {
        Value = value;
    }

    // Note: in entities with none default constructor, for setting constructor parameter, we need a private set property
    // when we didn't define this property in fluent configuration mapping (if so we can remove private set) , because for getting mapping list of properties to set
    // in the constructor it should not be read only without set (for bypassing calculate fields)- https://learn.microsoft.com/en-us/ef/core/modeling/constructors#read-only-properties
    public decimal Value { get; private set; }
    public static Amount Zero => Of(0);

    public static Amount Of(decimal value)
    {
        // validations should be placed here instead of constructor
        if (value is < 0 or > 1000000)
        {
            throw new InvalidAmountException(value);
        }

        return new Amount(value);
    }

    public static implicit operator decimal(Amount value) => value.Value;

    public static bool operator >(Amount a, Amount b) => a.Value > b.Value;

    public static bool operator <(Amount a, Amount b) => a.Value < b.Value;

    public static bool operator >=(Amount a, Amount b) => a.Value >= b.Value;

    public static bool operator <=(Amount a, Amount b) => a.Value <= b.Value;

    public static Amount operator +(Amount a, Amount b) => new(a.Value + b.Value);

    public static Amount operator -(Amount a, Amount b) => new(a.Value - b.Value);
}
