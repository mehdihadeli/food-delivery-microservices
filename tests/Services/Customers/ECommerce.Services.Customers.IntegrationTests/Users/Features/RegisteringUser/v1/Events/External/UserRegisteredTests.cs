using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Events;
using ECommerce.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;
using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.Users.Features.RegisteringUser.v1.Events.External;

public class UserRegisteredTests : CustomerServiceIntegrationTestBase
{
    private static UserRegisteredV1 _userRegistered = default!;

    public UserRegisteredTests(
        SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper)
        : base(sharedFixture, outputHelper)
    {
        _userRegistered = new FakeUserRegisteredV1().Generate();

        CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail(_userRegistered);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_consume_by_existing_consumer_through_the_broker()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, null, CancellationToken);

        // Assert
        await SharedFixture.WaitForConsuming<UserRegisteredV1>();
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
        await SharedFixture.WaitForConsuming<UserRegisteredV1, UserRegisteredConsumer>();
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
                var res = await ctx.Customers.AnyAsync(x => x.Email.Value == _userRegistered.Email);

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
        await SharedFixture.ShouldProcessedPersistInternalCommand<CreateMongoCustomerReadModels>();
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
                var res = await ctx.Customers.AsQueryable().AnyAsync(x => x.Email == _userRegistered.Email);

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
        await SharedFixture.ShouldProcessedOutboxPersistMessage<CustomerCreatedV1>();
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_publish_customer_created_integration_event_after_consuming_message()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await SharedFixture.WaitForPublishing<CustomerCreatedV1>();
    }
}
