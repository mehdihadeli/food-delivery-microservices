using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.ValueObjects;

public record Description
{
    // EF
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
