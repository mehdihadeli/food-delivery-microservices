using Ardalis.GuardClauses;

namespace ECommerce.Services.Orders.Orders.ValueObjects;

// https://ardalis.com/working-with-value-objects/
// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
public record ProductInfo
{
    // EF
    public ProductInfo()
    {
    }

    public string Name { get; private set; } = default!;
    public long ProductId { get; private set; }
    public decimal Price { get; private set; }

    public static ProductInfo Of(string name, long productId, decimal price)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.NegativeOrZero(productId);
        Guard.Against.NegativeOrZero(price);

        return new ProductInfo {Name = name, ProductId = productId, Price = price};
    }
}
