using BuildingBlocks.Core.Exception;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Shared.Customers.Customers.Events.Integration.v1;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.IntegrationTests.Customers.Features.CreatingCustomer.v1;

public class CreateCustomerTests(
    SharedFixtureWithEfCoreAndMongo<Api.CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture
) : CustomerServiceIntegrationTestBase(sharedFixture)
{
    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task can_create_new_customer_with_valid_input_in_postgres_db()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        var createdCustomerResponse = await SharedFixture.CommandAsync(command, TestContext.Current.CancellationToken);

        // Assert
        createdCustomerResponse.CustomerId.Should().BeGreaterThan(0);
        createdCustomerResponse.CustomerId.Should().Be(command.Id);
        createdCustomerResponse.IdentityUserId.Should().Be(fakeIdentityUser.Id);

        var createdCustomer = await SharedFixture.ExecuteEfDbContextAsync(async db =>
            await db.Customers.SingleOrDefaultAsync(x => x.Id == createdCustomerResponse.CustomerId)
        );

        createdCustomer.Should().NotBeNull();
        createdCustomer!.IdentityId.Should().Be(fakeIdentityUser.Id);
        createdCustomer.Email.Value.Should().Be(fakeIdentityUser.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task must_throw_exception_when_identity_user_with_customer_email_not_exists()
    {
        // Arrange
        var command = new CreateCustomer("test@example.com");

        // Act
        Func<Task> act = async () => await SharedFixture.CommandAsync(command);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(x => x.StatusCode == StatusCodes.Status404NotFound && !string.IsNullOrWhiteSpace(x.Message));
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task must_throw_exception_when_customer_with_email_already_exists()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        await SharedFixture.CommandAsync(command, TestContext.Current.CancellationToken);

        Func<Task> act = async () => await SharedFixture.CommandAsync(new CreateCustomer(fakeIdentityUser.Email));

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerAlreadyExistsException>();
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task can_save_mongo_customer_read_model_in_internal_persistence_message()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        await SharedFixture.CommandAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await SharedFixture.ShouldProcessingInternalCommand<CreateCustomerRead>(TestContext.Current.CancellationToken);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task can_create_new_mongo_customer_read_model_in_the_mongodb()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        await SharedFixture.CommandAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await SharedFixture.WaitUntilConditionMet(async () =>
        {
            var existsCustomer = await SharedFixture.ExecuteMongoDbContextAsync(async ctx =>
            {
                var res = ctx.Customers.AsQueryable().Any(x => x.Email == command.Email);

                return res;
            });

            return existsCustomer;
        });
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task can_publish_customer_created_integration_event_to_the_broker()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        var _ = await SharedFixture.CommandAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await SharedFixture.ShouldPublishing<CustomerCreatedV1>(TestContext.Current.CancellationToken);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task can_save_customer_created_integration_event_in_the_outbox()
    {
        // Arrange
        var fakeIdentityUser = IdentityServiceWireMock.SetupGetUserByEmail().Response.UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        var _ = await SharedFixture.CommandAsync(command, TestContext.Current.CancellationToken);

        // Assert
        await SharedFixture.ShouldProcessingOutboxMessage<CustomerCreatedV1>(TestContext.Current.CancellationToken);
    }
}
