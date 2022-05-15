using BuildingBlocks.Core.Domain;
using Store.Services.Orders.Orders.ValueObjects;

namespace Store.Services.Orders.Orders.Models;

public class Order : Aggregate<OrderId>
{
    public CustomerInfo Customer { get; private set; } = null!;
    public ProductInfo Product { get; private set; } = null!;

    public static Order Create(CustomerInfo customerInfo, ProductInfo productInfo)
    {
        //TODO: Complete order domain model
        return new Order
        {
            Customer = customerInfo,
            Product = productInfo
        };
    }
}
