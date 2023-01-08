using Ardalis.GuardClauses;

namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Money
{
    // EF
    private Money()
    {
    }

    public decimal Value { get; private set; }
    public string Currency { get; private set;} = default!;

    public static Money Of(decimal value, string currency)
    {
        // validations should be placed here instead of constructor
        Guard.Against.NegativeOrZero(value, nameof(value));
        Guard.Against.NullOrWhiteSpace(currency, nameof(currency));

        return new Money {Currency = currency, Value = value};
    }

    public static Money operator *(int left, Money right)
    {
        return Of(right.Value * left, right.Currency);
    }
}
