namespace BuildingBlocks.Integration.MassTransit.IntegrationTests;

// public class BusPublishTests_For_Identity : IntegrationTestBase<ECommerce.Services.Identity.Api.Program>
// {
//     public BusPublishTests_For_Identity(
//         IntegrationTestFixture<ECommerce.Services.Identity.Api.Program> integrationTestFixture,
//         ITestOutputHelper outputHelper) :
//         base(integrationTestFixture, outputHelper)
//     {
//     }
//
//     [Fact]
//     public async Task publish_user_registered_should_send_message_to_correct_exchange_and_consumer()
//     {
//         // should receive in customer service.
//         await IntegrationTestFixture.Bus.PublishAsync(new UserRegistered(
//             Guid.NewGuid(), $"{Guid.NewGuid()}@test.com",
//             "ss",
//             "ss", "ss",
//             new List<string> {"user"}
//         ), null, CancellationToken.None);
//
//         await Task.Delay(12000);
//     }
// }
//
// public class BusPublishTests_For_Catalogs : IntegrationTestBase<ECommerce.Services.Catalogs.Api.Program>
// {
//     public BusPublishTests_For_Catalogs(
//         IntegrationTestFixture<ECommerce.Services.Catalogs.Api.Program> integrationTestFixture,
//         ITestOutputHelper outputHelper) :
//         base(integrationTestFixture, outputHelper)
//     {
//     }
//
//     [Fact]
//     public async Task publish_product_created_should_send_message_to_correct_exchange_and_consumer()
//     {
//         // should receive in customer service.
//         await IntegrationTestFixture.Bus.PublishAsync(
//             new ProductCreated(10, "test product", 1, "ss", 10), null, CancellationToken.None);
//
//         await Task.Delay(12000);
//     }
// }
