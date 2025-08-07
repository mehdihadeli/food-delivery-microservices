using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Rest;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity.Rest;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Servers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;

namespace FoodDelivery.Services.Customers.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public class CustomerServiceEndToEndTestBase(
    SharedFixtureWithEfCoreAndMongo<Api.CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : EndToEndTestTestBase<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext>(sharedFixture)
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

    // We don't need to inject `CustomersServiceMockServersFixture` class fixture in the constructor because it initialized by `collection fixture` and its static properties are accessible in the codes
    // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
    // note1: for E2E test we use real identity service in on a TestContainer docker of this service, coordination with an external system is necessary in E2E
    // note2: add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
    // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
    protected override void OverrideInMemoryConfig(IDictionary<string, string> keyValues)
    {
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
