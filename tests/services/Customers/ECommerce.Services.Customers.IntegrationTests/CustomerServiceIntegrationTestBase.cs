using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class
    CustomerServiceIntegrationTestBase : IntegrationTestBase<Api.Program, CustomersDbContext,
        CustomersReadDbContext>, IClassFixture<MockServersFixture>
{
    public MockServersFixture MockServersFixture { get; }

    public CustomerServiceIntegrationTestBase(
        SharedFixture<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        MockServersFixture mockServersFixture,
        ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper)
    {
        MockServersFixture = mockServersFixture;

        // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
        // note1: for E2E test we use real identity service in on a TestContainer docker of this service, coordination with an external system is necessary in E2E

        // note2: add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
        // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        Fixture.Configuration["IdentityApiClientOptions:BaseApiAddress"] = MockServersFixture.IdentityServiceMock.Url;
        Fixture.Configuration["CatalogsApiClientOptions:BaseApiAddress"] = MockServersFixture.CatalogsServiceMock.Url;

        // var catalogApiOptions = Scope.ServiceProvider.GetRequiredService<IOptions<CatalogsApiClientOptions>>();
        // var identityApiOptions = Scope.ServiceProvider.GetRequiredService<IOptions<IdentityApiClientOptions>>();
        //
        // identityApiOptions.Value.BaseApiAddress = MockServersFixture.IdentityServiceMock.Url!;
        // catalogApiOptions.Value.BaseApiAddress = MockServersFixture.CatalogsServiceMock.Url!;
    }

    protected override void RegisterTestAppConfigurations(IConfigurationBuilder builder, IConfiguration configuration,
        IHostEnvironment environment)
    {
        base.RegisterTestAppConfigurations(builder, configuration, environment);
    }

    protected override void RegisterTestConfigureServices(IServiceCollection services)
    {
        //// here we use same data seeder of service but if we need different data seeder for test for can replace it
        // services.ReplaceScoped<IDataSeeder, CustomersTestDataSeeder>();
    }
}
