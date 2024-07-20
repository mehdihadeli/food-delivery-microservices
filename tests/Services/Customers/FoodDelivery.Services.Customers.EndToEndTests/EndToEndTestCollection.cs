using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fixtures;
using Tests.Shared.Fixtures;

namespace FoodDelivery.Services.Customers.EndToEndTests;

// https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection, but it can implements multiple ICollectionFixture in its definitions
[CollectionDefinition(Name)]
public class EndToEndTestCollection
    : ICollectionFixture<
        SharedFixtureWithEfCoreAndMongo<Api.CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext>
    >,
        ICollectionFixture<CustomersServiceMockServersFixture>
{
    public const string Name = "End-To-End Test";
}
