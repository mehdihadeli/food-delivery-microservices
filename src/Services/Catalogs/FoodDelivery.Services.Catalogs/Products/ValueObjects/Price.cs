// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
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
        value.NotBeNegativeOrZero();

        // validations should be placed here instead of constructor
        return new Price(value);
    }

    public static Price Of([NotNull] decimal? value)
    {
        value.NotBeNull();

        return Of(value.Value);
    }

    public static implicit operator decimal(Price value) => value.Value;

    public void Deconstruct(out decimal value) => value = Value;
}
