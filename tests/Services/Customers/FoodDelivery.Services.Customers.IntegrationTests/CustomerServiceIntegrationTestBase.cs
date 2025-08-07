using BuildingBlocks.Persistence.EfCore.Postgres;
using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;

namespace FoodDelivery.Services.Customers.IntegrationTests;

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(IntegrationTestCollection.Name)]
public class CustomerServiceIntegrationTestBase(
    SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture
) : IntegrationTestBase<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext>(sharedFixture)
{
    private IdentityServiceWireMock? _identityServiceWireMock;
    private CatalogsServiceWireMock? _catalogsServiceWireMock;

    public IdentityServiceWireMock IdentityServiceWireMock
    {
        get
        {
            if (_identityServiceWireMock is null)
            {
                var option = SharedFixture.ServiceProvider.GetRequiredService<IOptions<IdentityRestClientOptions>>();
                _identityServiceWireMock = new IdentityServiceWireMock(SharedFixture.WireMockServer, option.Value);
            }

            return _identityServiceWireMock;
        }
    }

    public CatalogsServiceWireMock CatalogsServiceWireMock
    {
        get
        {
            if (_catalogsServiceWireMock is null)
            {
                var option = SharedFixture.ServiceProvider.GetRequiredService<IOptions<CatalogsRestClientOptions>>();
                _catalogsServiceWireMock = new CatalogsServiceWireMock(SharedFixture.WireMockServer, option.Value);
            }

            return _catalogsServiceWireMock;
        }
    }

    protected override void OverrideInMemoryConfig(IDictionary<string, string> keyValues)
    {
        keyValues.Add(
            $"{nameof(PostgresOptions)}:{nameof(PostgresOptions.ConnectionString)}",
            SharedFixture.PostgresContainerFixture.ConnectionString
        );
        keyValues.Add(
            $"{nameof(IdentityRestClientOptions)}:{nameof(IdentityRestClientOptions.BaseAddress)}",
            SharedFixture.WireMockServerUrl
        );
        keyValues.Add(
            $"{nameof(CatalogsRestClientOptions)}:{nameof(IdentityRestClientOptions.BaseAddress)}",
            SharedFixture.WireMockServerUrl
        );
    }
}
