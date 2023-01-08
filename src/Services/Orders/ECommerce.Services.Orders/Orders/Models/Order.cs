using BuildingBlocks.Core.Domain;
using ECommerce.Services.Orders.Orders.ValueObjects;

namespace ECommerce.Services.Orders.Orders.Models;

// https://learn.microsoft.com/en-us/ef/core/modeling/constructors
// https://github.com/dotnet/efcore/issues/29940
public class Order : Aggregate<OrderId>
{
    // EF
    // this constructor is needed when we have a parameter constructor that has some navigation property classes in the parameters and ef will skip it and try to find other constructor, here default constructor (maybe will fix .net 8)
    private Order()
    {
    }

    public CustomerInfo Customer { get; private set; } = default!;
    public ProductInfo Product { get; private set; } = default!;

    public static Order Create(CustomerInfo customerInfo, ProductInfo productInfo)
    {
        //TODO: Complete order domain model
        return new Order {Customer = customerInfo, Product = productInfo};
    }
}
