using ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using Tests.Shared.Helpers;

namespace ECommerce.Services.Customers.IntegrationTests;

public class MockServersFixture : IAsyncLifetime
{
    public  IdentityServiceMock IdentityServiceMock { get; private set; }
    public  CatalogsServiceMock CatalogsServiceMock { get; private set; }

    public MockServersFixture()
    {
        IdentityServiceMock = IdentityServiceMock.Start(ConfigurationHelper.GetOptions<IdentityApiClientOptions>());
        CatalogsServiceMock = CatalogsServiceMock.Start(ConfigurationHelper.GetOptions<CatalogsApiClientOptions>());
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        IdentityServiceMock.Dispose();
        CatalogsServiceMock.Dispose();

        return Task.CompletedTask;
    }
}
