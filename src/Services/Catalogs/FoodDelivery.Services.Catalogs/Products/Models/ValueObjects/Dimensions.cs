using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;

// https://github.com/NimblePros/ValueObjects
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net
// Records are immutable reference types and their support equality comparison out of the box. They compare based on their properties, not by reference. Records also have a ready "ToString" method out of the box, that outputs all the properties in a readable way.
public record Dimensions
{
    // For EF materialization - No validation
    // Value object constraints should not be enforced in EF Core materialization and should be enforced during application-level creation with validations (Of)
    private Dimensions() { }

    public int Height { get; private set; }
    public int Width { get; private set; }
    public int Depth { get; private set; }

    public static Dimensions Of(int width, int height, int depth)
    {
        height.NotBeNegativeOrZero();
        width.NotBeNegativeOrZero();
        depth.NotBeNegativeOrZero();

        return new Dimensions
        {
            Height = height,
            Width = width,
            Depth = depth,
        };
    }

    public void Deconstruct(out int width, out int height, out int depth) =>
        (width, height, depth) = (Width, Height, Depth);

    public override string ToString()
    {
        return FormattedDescription();
    }

    private string FormattedDescription()
    {
        return $"HxWxD: {Height} x {Width} x {Depth}";
    }
}
