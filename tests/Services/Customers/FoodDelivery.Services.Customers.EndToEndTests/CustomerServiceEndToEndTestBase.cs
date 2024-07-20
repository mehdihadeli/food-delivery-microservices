using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fixtures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace FoodDelivery.Services.Customers.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
public class CustomerServiceEndToEndTestBase
    : EndToEndTestTestBase<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext>
{
    // We don't need to inject `CustomersServiceMockServersFixture` class fixture in the constructor because it initialized by `collection fixture` and its static properties are accessible in the codes
    public CustomerServiceEndToEndTestBase(
        SharedFixtureWithEfCoreAndMongo<
            Api.CustomersApiMetadata,
            CustomersDbContext,
            CustomersReadDbContext
        > sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper)
    {
        // https://pcholko.com/posts/2021-04-05/wiremock-integration-test/
        // note1: for E2E test we use real identity service in on a TestContainer docker of this service, coordination with an external system is necessary in E2E

        // note2: add in-memory configuration instead of using appestings.json and override existing settings and it is accessible via IOptions and Configuration
        // https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        SharedFixture.Configuration["IdentityApiClientOptions:BaseApiAddress"] = CustomersServiceMockServersFixture
            .IdentityServiceMock
            .Url;
        SharedFixture.Configuration["CatalogsApiClientOptions:BaseApiAddress"] = CustomersServiceMockServersFixture
            .CatalogsServiceMock
            .Url;

        // var catalogApiOptions = Scope.ServiceProvider.GetRequiredService<IOptions<CatalogsApiClientOptions>>();
        // var identityApiOptions = Scope.ServiceProvider.GetRequiredService<IOptions<IdentityApiClientOptions>>();
        //
        // identityApiOptions.Value.BaseApiAddress = MockServersFixture.IdentityServiceMock.Url!;
        // catalogApiOptions.Value.BaseApiAddress = MockServersFixture.CatalogsServiceMock.Url!;
    }

    protected override void RegisterTestAppConfigurations(
        IConfigurationBuilder builder,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        base.RegisterTestAppConfigurations(builder, configuration, environment);
    }

    protected override void RegisterTestConfigureServices(IServiceCollection services)
    {
        //// here we use same data seeder of service but if we need different data seeder for test for can replace it
        // services.ReplaceScoped<IDataSeeder, CustomersTestDataSeeder>();
    }

    public override Task DisposeAsync()
    {
        // we should reset mappings routes we define in each test in end of running each test, but wiremock server is up in whole of test collection and is active for all tests
        CustomersServiceMockServersFixture.CatalogsServiceMock.Reset();
        CustomersServiceMockServersFixture.IdentityServiceMock.Reset();

        return base.DisposeAsync();
    }
}
