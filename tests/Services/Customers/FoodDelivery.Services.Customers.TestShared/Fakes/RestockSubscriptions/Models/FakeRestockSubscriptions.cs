using Bogus;
using BuildingBlocks.Core.Domain.ValueObjects;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Models;

// Note: AutoBogus doesn't generate values for readonly properties (propertyInfo.CanWrite == false in reflection)
// Note that, should a rule set be used to generate a type, then only members not defined in the rule set are auto generated.
// https://github.com/nickdodd79/AutoBogus#autofakert
// `Faker` has a problem with non-default constructor but `AutoFaker` works also with none-default constructor
// because AutoFaker generate data also for private set and init members (not read only get) it doesn't work properly with `CustomInstantiator` and we should exclude theme one by one, so it is better we use Faker<>
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
