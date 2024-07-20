using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Orders.Orders.ValueObjects;

// https://ardalis.com/working-with-value-objects/
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record ProductInfo
{
    // EF
    private ProductInfo() { }

    public string Name { get; private set; } = default!;
    public long ProductId { get; private set; }
    public decimal Price { get; private set; }

    public static ProductInfo Of([NotNull] string? name, long productId, decimal price)
    {
        return new ProductInfo
        {
            Name = name.NotBeEmptyOrNull(),
            ProductId = productId.NotBeNegativeOrZero(),
            Price = price.NotBeNegativeOrZero()
        };
    }

    public void Deconstruct(out string name, out long productId, out decimal price) =>
        (name, productId, price) = (Name, ProductId, Price);
}
