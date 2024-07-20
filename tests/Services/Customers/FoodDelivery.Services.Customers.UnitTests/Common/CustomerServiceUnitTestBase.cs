using AutoMapper;
using BuildingBlocks.Resiliency;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fixtures;
using Microsoft.Extensions.Options;
using Tests.Shared.Helpers;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Common;

[CollectionDefinition(nameof(QueryTestCollection))]
public class QueryTestCollection : ICollectionFixture<CustomerServiceUnitTestBase> { }

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(UnitTestCollection.Name)]
[CategoryTrait(TestCategory.Unit)]
public class CustomerServiceUnitTestBase : IAsyncDisposable
{
    // We don't need to inject `CustomersServiceMockServersFixture` class fixture in the constructor because it initialized by `collection fixture` and its static properties are accessible in the codes
    public CustomerServiceUnitTestBase()
    {
        Mapper = MapperFactory.Create();
        CustomersDbContext = DbContextFactory.Create();

        //https://stackoverflow.com/questions/40876507/net-core-unit-testing-mock-ioptionst
        IOptions<IdentityApiClientOptions> identityClientOptions = Options.Create(
            ConfigurationHelper.BindOptions<IdentityApiClientOptions>()
        );
        IOptions<PolicyOptions> policyOptions = Options.Create(
            new PolicyOptions
            {
                RetryCount = 1,
                TimeOutDuration = 3,
                BreakDuration = 5
            }
        );
        IdentityApiClient = new IdentityApiClient(
            new HttpClient { BaseAddress = new Uri(CustomersServiceMockServersFixture.IdentityServiceMock.Url!) },
            Mapper,
            identityClientOptions,
            policyOptions
        );

        //https://stackoverflow.com/questions/40876507/net-core-unit-testing-mock-ioptionst
        IOptions<CatalogsApiClientOptions> catalogClientOptions = Options.Create(
            ConfigurationHelper.BindOptions<CatalogsApiClientOptions>()
        );
        CatalogApiClient = new CatalogApiClient(
            new HttpClient { BaseAddress = new Uri(CustomersServiceMockServersFixture.CatalogsServiceMock.Url!) },
            Mapper,
            catalogClientOptions,
            policyOptions
        );
    }

    public IMapper Mapper { get; }
    public CustomersDbContext CustomersDbContext { get; }
    public IdentityApiClient IdentityApiClient { get; }
    public CatalogApiClient CatalogApiClient { get; }

    public async ValueTask DisposeAsync()
    {
        await DbContextFactory.Destroy(CustomersDbContext);
        // we should reset mappings routes we define in each test in end of running each test, but wiremock server is up in whole of test collection and is active for all tests
        CustomersServiceMockServersFixture.CatalogsServiceMock.Reset();
        CustomersServiceMockServersFixture.IdentityServiceMock.Reset();
    }
}
