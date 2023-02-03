using ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using Tests.Shared.Helpers;

namespace ECommerce.Services.Customers.IntegrationTests;

public class CustomersServiceMockServersFixture : IAsyncLifetime
{
    public static IdentityServiceMock IdentityServiceMock { get; private set; } = default!;
    public static CatalogsServiceMock CatalogsServiceMock { get; private set; } = default!;

    public Task InitializeAsync()
    {
        IdentityServiceMock = IdentityServiceMock.Start(ConfigurationHelper.BindOptions<IdentityApiClientOptions>());
        CatalogsServiceMock = CatalogsServiceMock.Start(ConfigurationHelper.BindOptions<CatalogsApiClientOptions>());

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        IdentityServiceMock.Dispose();
        CatalogsServiceMock.Dispose();

        return Task.CompletedTask;
    }
}
