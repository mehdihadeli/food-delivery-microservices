using Bogus;
using Store.Services.Shared.Catalogs.Products.Events.Integration;
using Store.Services.Shared.Identity.Users.Events.Integration;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace IntegrationTests;

public class BusPublishTests_For_Identity : IntegrationTestBase<Store.Services.Identity.Api.Program>
{
    public BusPublishTests_For_Identity(
        IntegrationTestFixture<Store.Services.Identity.Api.Program> integrationTestFixture,
        ITestOutputHelper outputHelper) :
        base(integrationTestFixture, outputHelper)
    {
    }

    [Fact]
    public async Task publish_user_registered_should_send_message_to_correct_exchange_and_consumer()
    {
        // should receive in customer service.
        await IntegrationTestFixture.Bus.PublishAsync(new UserRegistered(
            Guid.NewGuid(), $"{Guid.NewGuid()}@test.com",
            "ss",
            "ss", "ss",
            new List<string> {"user"}
        ), null, CancellationToken.None);

        await Task.Delay(12000);
    }
}

public class BusPublishTests_For_Catalogs : IntegrationTestBase<Store.Services.Catalogs.Api.Program>
{
    public BusPublishTests_For_Catalogs(
        IntegrationTestFixture<Store.Services.Catalogs.Api.Program> integrationTestFixture,
        ITestOutputHelper outputHelper) :
        base(integrationTestFixture, outputHelper)
    {
    }

    [Fact]
    public async Task publish_product_created_should_send_message_to_correct_exchange_and_consumer()
    {
        // should receive in customer service.
        await IntegrationTestFixture.Bus.PublishAsync(
            new ProductCreated(10, "test product", 1, "ss", 10), null, CancellationToken.None);

        await Task.Delay(12000);
    }
}
