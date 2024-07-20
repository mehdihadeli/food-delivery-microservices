using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.ValueObjects;

// https://github.com/NimblePros/ValueObjects
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
public record Dimensions
{
    // EF
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

    public static Dimensions Of([NotNull] int? width, [NotNull] int? height, [NotNull] int? depth)
    {
        height.NotBeNull();
        width.NotBeNull();
        depth.NotBeNull();

        return Of(width.Value, height.Value, depth.Value);
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
