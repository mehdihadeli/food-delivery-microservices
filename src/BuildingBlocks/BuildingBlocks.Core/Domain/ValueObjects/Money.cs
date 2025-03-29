using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public record Money
{
    // EF
    private Money() { }

    public decimal Value { get; private set; }
    public string Currency { get; private set; } = default!;

    public static Money Of(decimal value, string currency)
    {
        // validations should be placed here instead of constructor
        value.NotBeNegativeOrZero();
        currency.NotBeNullOrWhiteSpace();
        currency.NotBeInvalidCurrency();

        return new Money { Currency = currency, Value = value };
    }

    public static Money operator *(int left, Money right)
    {
        return Of(right.Value * left, right.Currency);
    }

    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
    // https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
    public void Deconstruct(out decimal value, out string currency) => (value, currency) = (Value, Currency);
}
