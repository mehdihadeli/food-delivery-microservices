using Bogus;
using ECommerce.Services.Customers.Customers.Features;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Clients.Identity.Dtos;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Users.Features.RegisteringUser.Events.External;
using ECommerce.Services.Shared.Customers.Customers.Events.Integration;
using ECommerce.Services.Shared.Identity.Users.Events.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.Users.Features.RegisteringUser.Events.External;

public class UserRegisteredTests : IntegrationTestBase<Program, CustomersDbContext, CustomersReadDbContext>
{
    private static UserRegistered _userRegistered;

    public UserRegisteredTests(
        IntegrationTestFixture<Program, CustomersDbContext, CustomersReadDbContext> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
        _userRegistered = new Faker<UserRegistered>().CustomInstantiator(faker =>
                new UserRegistered(
                    Guid.NewGuid(),
                    faker.Person.Email,
                    faker.Person.UserName,
                    faker.Person.FirstName,
                    faker.Person.LastName, new List<string> {"user"}))
            .Generate();
    }

    protected override void RegisterTestsServices(IServiceCollection services)
    {
        base.RegisterTestsServices(services);

        services.Replace(ServiceDescriptor.Transient<IIdentityApiClient>(x =>
        {
            var f = Substitute.For<IIdentityApiClient>();
            f.GetUserByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())!
                .Returns(args =>
                {
                    var email = args.Arg<string>();

                    return Task.FromResult(new GetUserByEmailResponse(new UserIdentityDto()
                    {
                        Email = _userRegistered.Email,
                        Id = _userRegistered.IdentityId,
                        FirstName = _userRegistered.FirstName,
                        LastName = _userRegistered.LastName,
                        UserName = _userRegistered.UserName
                    }));
                });

            return f;
        }));

        // services.ReplaceScoped<IDataSeeder, CustomersTestDataSeeder>();
    }

    [Fact]
    public async Task user_registered_message_should_consume_existing_consumer_by_broker()
    {
        // Act
        await IntegrationTestFixture.PublishMessageAsync(_userRegistered, null, CancellationToken);

        // Assert
        var shouldConsume = await IntegrationTestFixture.IsConsumed<UserRegistered>();
        shouldConsume.Should().BeTrue();
    }

    // [Fact]
    // public async Task user_registered_message_should_consume_new_consumers_by_broker()
    // {
    //     // Arrange
    //     var shouldConsume = await IntegrationTestFixture.ShouldConsumeWithNewConsumer<UserRegistered>();
    //
    //     // Act
    //     await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);
    //
    //     // Assert
    //     await shouldConsume.Validate(60.Seconds());
    // }
    //
    [Fact]
    public async Task user_registered_message_should_consume_by_user_registered_consumer()
    {
        // Act
        await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        var shouldConsume = await IntegrationTestFixture.IsConsumed<UserRegistered, UserRegisteredConsumer>();
        shouldConsume.Should().BeTrue();
    }

    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_postgres_write_db()
    {
        // Act
        await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await IntegrationTestFixture.WaitUntilConditionMet(async () =>
        {
            var existsCustomer = await IntegrationTestFixture.ExecuteContextAsync(async ctx =>
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
        await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await IntegrationTestFixture.ShouldProcessedPersistInternalCommand<CreateMongoCustomerReadModels>();

        var existsCustomer = await IntegrationTestFixture.ExecuteReadContextAsync(async ctx =>
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
        await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        await IntegrationTestFixture.ShouldProcessedOutboxPersistMessage<CustomerCreated>();
    }

    [Fact]
    public async Task user_registered_should_should_publish_customer_created()
    {
        // Act
        await IntegrationTestFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await IntegrationTestFixture.WaitForPublishing<CustomerCreated>();
    }
}
