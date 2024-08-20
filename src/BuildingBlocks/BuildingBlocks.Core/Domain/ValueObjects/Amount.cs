using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Extensions;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace BuildingBlocks.Core.Domain.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
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

    public static Amount Of([NotNull] decimal? value)
    {
        value.NotBeNull();

        return Of(value.Value);
    }

    public static Amount Of([NotNull] decimal value)
    {
        value.NotBeNegativeOrZero();

        // validations should be placed here instead of constructor
        if (value > 1000000)
        {
            throw new InvalidAmountException(value);
        }

        return new Amount(value);
    }

    public static implicit operator decimal(Amount? value) => value?.Value ?? default;

    public static bool operator >(Amount a, Amount b) => a.Value > b.Value;

    public static bool operator <(Amount a, Amount b) => a.Value < b.Value;

    public static bool operator >=(Amount a, Amount b) => a.Value >= b.Value;

    public static bool operator <=(Amount a, Amount b) => a.Value <= b.Value;

    public static Amount operator +(Amount a, Amount b) => new(a.Value + b.Value);

    public static Amount operator -(Amount a, Amount b) => new(a.Value - b.Value);

    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct#user-defined-types
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#positional-syntax-for-property-definition
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation
    // https://alexanderzeitler.com/articles/deconstructing-a-csharp-record-with-properties/
    public void Deconstruct(out decimal value) => value = Value;
}
