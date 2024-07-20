using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Brands.ValueObjects;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public record BrandName
{
    // EF
    private BrandName() { }

    public string Value { get; private set; } = default!;

    public static BrandName Of([NotNull] string? value)
    {
        // validations should be placed here instead of constructor
        value.NotBeNullOrWhiteSpace();

        return new BrandName { Value = value };
    }

    public static implicit operator string(BrandName value) => value.Value;

    public void Deconstruct(out string value) => value = Value;
}
