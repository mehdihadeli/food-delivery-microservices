using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;

public record Description
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private Description() { }

    public string Value { get; private set; } = default!;

    public static Description Of(string value)
    {
        // validations should be placed here instead of constructor
        value.NotBeNullOrWhiteSpace();

        return new Description { Value = value };
    }

    public static implicit operator string(Description value) => value.Value;

    public void Deconstruct(out string value) => value = Value;
}
