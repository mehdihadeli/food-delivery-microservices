using Ardalis.GuardClauses;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Products.Exceptions.Domain;

namespace ECommerce.Services.Catalogs.Products.ValueObjects;

// https://github.com/NimblePros/ValueObjects
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record Dimensions
{
    // EF
    public Dimensions()
    {
    }

    public int Height { get; private set; }
    public int Width { get; private set; }
    public int Depth { get; private set; }

    public static Dimensions Of(int width, int height, int depth)
    {
        Guard.Against.NegativeOrZero(height);
        Guard.Against.NegativeOrZero(width);
        Guard.Against.NegativeOrZero(depth);

        return new Dimensions {Height = height, Width = width, Depth = depth,};
    }

    public override string ToString()
    {
        return FormattedDescription();
    }

    private string FormattedDescription()
    {
        return $"HxWxD: {Height} x {Width} x {Depth}";
    }
}
