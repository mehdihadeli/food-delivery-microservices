using BuildingBlocks.Core.Extensions;
using ECommerce.Services.Customers.IntegrationTests.Mocks.Servers;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests;

// [Collection(nameof(IntegrationTestFixture<Program>))]
public class
    CustomerServiceIntegrationTestBase : IntegrationTestBase<Api.Program, CustomersDbContext, CustomersReadDbContext>
{
    protected IdentityServiceMock IdentityServiceMock { get; private set; } = default!;
    protected CatalogsServiceMock CatalogsServiceMock { get; private set; } = default!;

    public CustomerServiceIntegrationTestBase(
        CustomWebApplicationFactory<Api.Program> webApplicationFactory,
        SharedFixture sharedFixture,
        ITestOutputHelper outputHelper)
        : base(webApplicationFactory, sharedFixture, outputHelper)
    {
    }

    protected override void RegisterTestAppConfigurations(
        IConfigurationBuilder builder,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
        // note1: for E2E test we use real identity service in on a TestContainer docker of this service, coordination with an external system is necessary in E2E

        // note2: add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
        // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        // builder.AddInMemoryCollection(
        //     new Dictionary<string, string?>
        //     {
        //         {"IdentityApiClientOptions:BaseApiAddress", WireMockServerUrl},
        //         {"CatalogsApiClientOptions:BaseApiAddress", WireMockServerUrl},
        //     });

        // Or instead of AddInMemoryCollection we can set values configuration explicitly
        IdentityServiceMock = IdentityServiceMock.Start(configuration.GetOptions<IdentityApiClientOptions>());
        CatalogsServiceMock = CatalogsServiceMock.Start(configuration.GetOptions<CatalogsApiClientOptions>());

        configuration["IdentityApiClientOptions:BaseApiAddress"] = IdentityServiceMock.Url;
        configuration["CatalogsApiClientOptions:BaseApiAddress"] = CatalogsServiceMock.Url;
    }

    protected override void RegisterTestConfigureServices(IServiceCollection services)
    {
        //// here we use same data seeder of service but if we need different data seeder for test for can replace it
        // services.ReplaceScoped<IDataSeeder, CustomersTestDataSeeder>();
    }
}
