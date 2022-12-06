using ECommerce.Services.Customers.Customers.Features;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;
using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Tests.Shared.Fakes.Customers.Customers.Events;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.Users.Features.RegisteringUser.v1.Events.External;

public class UserRegisteredTests : CustomerServiceIntegrationTestBase
{
    private static UserRegisteredV1 _userRegistered = default!;

    public UserRegisteredTests(
        SharedFixture<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        MockServersFixture mockServersFixture,
        ITestOutputHelper outputHelper)
        : base(sharedFixture, mockServersFixture, outputHelper)
    {
        _userRegistered = new FakeUserRegisteredV1().Generate();
        MockServersFixture.IdentityServiceMock.SetupGetUserByEmail(_userRegistered.Email,
            response: new GetUserByEmailResponse(new UserIdentityDto
            {
                Email = _userRegistered.Email,
                Id = _userRegistered.IdentityId,
                FirstName = _userRegistered.FirstName,
                LastName = _userRegistered.LastName,
                UserName = _userRegistered.UserName,
            }));
    }

    [Fact]
    public async Task message_should_consume_by_existing_consumer_through_broker()
    {
        // Act
        await Fixture.PublishMessageAsync(_userRegistered, null, CancellationToken);

        // Assert
        await Fixture.WaitForConsuming<UserRegisteredV1>();
        await Fixture.WaitForPublishing<CustomerCreatedV1>();
    }

    // [Fact]
    // public async Task message_should_consume_by_new_consumers_through_broker()
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
    public async Task message_should_consume_by_user_registered_consumer()
    {
        // Act
        await Fixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await Fixture.WaitForConsuming<UserRegisteredV1, UserRegisteredConsumer>();
        await Fixture.WaitForPublishing<CustomerCreatedV1>();
    }

    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_postgres_write_db()
    {
        // Act
        await Fixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await Fixture.WaitUntilConditionMet(async () =>
        {
            var existsCustomer = await Fixture.ExecuteContextAsync(async ctx =>
            {
                var res = await ctx.Customers.AnyAsync(x => x.Email == _userRegistered.Email.ToLower());

                return res;
            });

            return existsCustomer;
        });
    }

    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_internal_persistence_message_and_mongo()
    {
        // Act
        await Fixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await Fixture.ShouldProcessedPersistInternalCommand<CreateMongoCustomerReadModels>();

        var existsCustomer = await Fixture.ExecuteReadContextAsync(async ctx =>
        {
            var res = await ctx.Customers.AsQueryable().AnyAsync(x => x.Email == _userRegistered.Email.ToLower());

            return res;
        });

        existsCustomer.Should().BeTrue();
    }

    [Fact]
    public async Task user_registered_message_should_create_customer_created_in_the_outbox()
    {
        // Act
        await Fixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        await Fixture.ShouldProcessedOutboxPersistMessage<CustomerCreatedV1>();
    }

    [Fact]
    public async Task user_registered_should_should_publish_customer_created()
    {
        // Act
        await Fixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await Fixture.WaitForPublishing<CustomerCreatedV1>();
    }
}
