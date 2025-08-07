using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Events;
using FoodDelivery.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using FoodDelivery.Services.Shared.Identity.Users.Events.Integration.v1;
using MongoDB.Driver;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.IntegrationTests.Users.Features.RegisteringUser.v1.Events.External;

public class UserRegisteredTests : CustomerServiceIntegrationTestBase
{
    private static UserRegisteredV1 _userRegistered = default!;

    public UserRegisteredTests(
        SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture
    )
        : base(sharedFixture)
    {
        _userRegistered = new FakeUserRegisteredV1().Generate();

        IdentityServiceWireMock.SetupGetUserByEmail(_userRegistered);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_consume_by_existing_consumer_through_the_broker()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        await SharedFixture.ShouldConsuming<UserRegisteredV1>(TestContext.Current.CancellationToken);
    }

    // [Fact]
    // [CategoryTrait(TestCategory.Integration)]
    // public async Task should_consume_by_new_consumers_through_broker()
    // {
    //     // Arrange
    //     var shouldConsume = await IntegrationTestFixture.ShouldConsumeWithNewConsumer<UserRegisteredV1>();
    //
    //     // Act
    //     await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);
    //
    //     // Assert
    //     await shouldConsume.Validate(60.Seconds());
    // }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_consume_by_user_registered_consumer_through_the_broker()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.ShouldConsuming<UserRegisteredV1, UserRegisteredConsumer>(
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_create_new_customer_in_postgres_write_db_when_after_consuming_message()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.WaitUntilConditionMet(async () =>
        {
            var existsCustomer = await SharedFixture.ExecuteEfDbContextAsync(async ctx =>
            {
                var res = ctx.Customers.Any(x => x.Email.Value == _userRegistered.Email);

                return res;
            });

            return existsCustomer;
        });
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_save_mongo_customer_read_model_in_internal_persistence_message_after_consuming_message()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.ShouldProcessingInternalCommand<CreateCustomerRead>(TestContext.Current.CancellationToken);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_create_new_mongo_customer_read_model_in_the_mongodb_after_consuming_message()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.WaitUntilConditionMet(async () =>
        {
            var existsCustomer = await SharedFixture.ExecuteMongoDbContextAsync(async ctx =>
            {
                var res = ctx.Customers.AsQueryable().Any(x => x.Email == _userRegistered.Email);

                return res;
            });

            return existsCustomer;
        });
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_save_customer_created_integration_event_in_the_outbox_after_consuming_message()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.ShouldProcessingOutboxMessage<CustomerCreatedV1>(TestContext.Current.CancellationToken);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_publish_customer_created_integration_event_after_consuming_message()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.ShouldPublishing<CustomerCreatedV1>(TestContext.Current.CancellationToken);
    }
}
