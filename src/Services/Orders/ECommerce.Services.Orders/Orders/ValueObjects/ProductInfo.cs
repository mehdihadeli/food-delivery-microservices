using BuildingBlocks.Core.Domain;

namespace ECommerce.Services.Orders.Orders.ValueObjects;

// https://ardalis.com/working-with-value-objects/
public class ProductInfo : ValueObject
{
    public string Name { get; private set; }
    public long ProductId { get; private set; }
    public decimal Price { get; private set; }

    public static ProductInfo Create(string name, long productId, decimal price)
    {
        return new ProductInfo { Name = name, ProductId = productId, Price = price };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return ProductId;
    }
}
