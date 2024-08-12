using Bogus;
using BuildingBlocks.Core.Domain.ValueObjects;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Entities;

public sealed class FakeRestockSubscriptions : Faker<RestockSubscription>
{
    public FakeRestockSubscriptions()
    {
        long id = 1;

        // we should not instantiate customer aggregate manually because it is possible we break aggregate invariant in creating a customer, and it is better we
        // create a customer with its factory method
        CustomInstantiator(f =>
            RestockSubscription.Create(
                RestockSubscriptionId.Of(id++),
                CustomerId.Of(id++),
                ProductInformation.Of(ProductId.Of(f.Random.Number(1, 100)), f.Commerce.ProductName()),
                Email.Of(f.Internet.Email())
            )
        );
    }
}
