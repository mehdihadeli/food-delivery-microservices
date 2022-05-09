using Ardalis.GuardClauses;

namespace BuildingBlocks.Core.Domain.ValueObjects;

public record Money
{
    public static Money? Null => null;

    public decimal Value { get; private set; }

    public string Currency { get; private set; }

    public static Money Create(decimal value, string currency)
    {
        return new Money()
        {
            Value = Guard.Against.NegativeOrZero(value, nameof(value)),
            Currency = Guard.Against.NullOrWhiteSpace(currency, nameof(currency))
        };
    }

    public static Money operator *(int left, Money right)
    {
        return Create(right.Value * left, right.Currency);
    }
}
