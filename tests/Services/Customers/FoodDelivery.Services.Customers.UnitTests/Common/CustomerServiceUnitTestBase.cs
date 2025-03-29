using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity;
using FoodDelivery.Services.Customers.Shared.Data;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Common;

[CollectionDefinition(nameof(QueryTestCollection))]
public class QueryTestCollection : ICollectionFixture<CustomerServiceUnitTestBase>;

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[CategoryTrait(TestCategory.Unit)]
public class CustomerServiceUnitTestBase : IAsyncDisposable
{
    // We don't need to inject `CustomersServiceMockServersFixture` class fixture in the constructor because it initialized by `collection fixture` and its static properties are accessible in the codes

    public CustomersDbContext CustomersDbContext { get; } = DbContextFactory.Create();
    public IIdentityRestClient IdentityRestClient { get; } = Substitute.For<IIdentityRestClient>();
    public ICatalogsRestClient IICatalogsClient { get; } = Substitute.For<ICatalogsRestClient>();

    public async ValueTask DisposeAsync()
    {
        await DbContextFactory.Destroy(CustomersDbContext);
    }
}
