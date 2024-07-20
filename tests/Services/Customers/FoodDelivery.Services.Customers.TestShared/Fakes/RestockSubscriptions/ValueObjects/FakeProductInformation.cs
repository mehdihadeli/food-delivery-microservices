using AutoBogus;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.ValueObjects;

public sealed class FakeProductInformation : AutoFaker<ProductInformation>
{
    public FakeProductInformation()
    {
        RuleFor(x => x.Id, f => ProductId.Of(f.Random.Number(1, 100)));
    }
}
