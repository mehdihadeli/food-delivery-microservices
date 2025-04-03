using AutoBogus;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

namespace FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Commands;

public sealed class FakeCreateMongoRestockSubscriptionReadModels : AutoFaker<CreateMongoRestockSubscriptionReadModels>
{
    public FakeCreateMongoRestockSubscriptionReadModels()
    {
        RuleFor(x => x.RestockSubscriptionId, f => f.Random.Long(1, 1000));
        RuleFor(x => x.CustomerId, f => f.Random.Long(1, 1000));
        RuleFor(x => x.CustomerName, f => f.Person.FullName);
        RuleFor(x => x.ProductId, f => f.Random.Long(1, 1000));
        RuleFor(x => x.ProductName, f => f.Commerce.ProductName());
        RuleFor(x => x.Email, f => f.Internet.Email());
        RuleFor(x => x.Processed, f => f.Random.Bool());
        RuleFor(x => x.ProcessedTime, f => f.Date.PastOffset().DateTime);
        RuleFor(x => x.IsDeleted, f => f.Random.Bool());
    }
}
