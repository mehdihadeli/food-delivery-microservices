using Store.Services.Orders.Orders.ValueObjects;

namespace Store.Services.Orders.Orders.Models;

public class Order
{
    public CustomerInfo Customer { get; private set; }
    public ProductInfo Product { get; private set; }
}
