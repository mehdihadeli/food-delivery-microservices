using AutoMapper;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
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
    public CustomerServiceUnitTestBase()
    {
        Mapper = MapperFactory.Create();
        CustomersDbContext = DbContextFactory.Create();

        IdentityApiClient = Substitute.For<IIdentityApiClient>();
        CatalogApiClient = Substitute.For<ICatalogApiClient>();
    }

    public IMapper Mapper { get; }
    public CustomersDbContext CustomersDbContext { get; }
    public IIdentityApiClient IdentityApiClient { get; }
    public ICatalogApiClient CatalogApiClient { get; }

    public async ValueTask DisposeAsync()
    {
        await DbContextFactory.Destroy(CustomersDbContext);
    }
}
