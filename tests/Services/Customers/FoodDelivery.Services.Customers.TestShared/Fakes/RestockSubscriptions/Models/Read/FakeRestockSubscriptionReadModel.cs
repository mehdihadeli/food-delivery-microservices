using AutoBogus;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Models.Read;

public sealed class FakeRestockSubscriptionReadModel : AutoFaker<RestockSubscriptionReadModel>
{
    public FakeRestockSubscriptionReadModel()
    {
        RuleFor(x => x.Email, f => f.Internet.Email());
        RuleFor(x => x.ProductId, f => f.Random.Long(1, 1000));
        RuleFor(x => x.CustomerId, f => f.Random.Long(1, 1000));
        RuleFor(x => x.RestockSubscriptionId, f => f.Random.Long(1, 1000));
    }
}
