using ECommerce.Services.Customers.Shared.Data;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests;

//https://stackoverflow.com/questions/43082094/use-multiple-collectionfixture-on-my-test-class-in-xunit-2-x
// note: each class could have only one collection
[Collection(IntegrationTestCollection.Name)]
public class
    CustomerServiceIntegrationTestBase : IntegrationTestBase<Api.Program, CustomersDbContext, CustomersReadDbContext>
{
    public CustomerServiceIntegrationTestBase(
        SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper)
    {
        // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
        // note1: for E2E test we use real identity service in on a TestContainer docker of this service, coordination with an external system is necessary in E2E

        // note2: add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
        // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        SharedFixture.Configuration["IdentityApiClientOptions:BaseApiAddress"] =
            CustomersServiceMockServersFixture.IdentityServiceMock.Url;
        SharedFixture.Configuration["CatalogsApiClientOptions:BaseApiAddress"] =
            CustomersServiceMockServersFixture.CatalogsServiceMock.Url;

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
