using ECommerce.Services.Customers.TestShared.Fakes.Shared.Servers;
using ECommerce.Services.Customers.TestShared.Fixtures;

namespace ECommerce.Services.Customers.UnitTests;

// https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection, but it can implements multiple ICollectionFixture in its definitions
[CollectionDefinition(Name)]
public class UnitTestCollection : ICollectionFixture<CustomersServiceMockServersFixture>
{
    public const string Name = "UnitTest Test";
}
