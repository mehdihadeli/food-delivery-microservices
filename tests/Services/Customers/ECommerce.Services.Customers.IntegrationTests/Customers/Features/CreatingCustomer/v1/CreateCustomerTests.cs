using System.Net;
using BuildingBlocks.Core.Exception.Types;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.Customers.Features.CreatingCustomer.v1;

public class CreateCustomerTests : CustomerServiceIntegrationTestBase
{
    public CreateCustomerTests(
        SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper) : base(sharedFixture, outputHelper)
    {
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_create_new_customer_in_postgres_db()
    {
        // Arrange
        var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail().Response
            .UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        var createdCustomerResponse = await SharedFixture.SendAsync(command);

        // Assert
        createdCustomerResponse.CustomerId.Should().BeGreaterThan(0);
        createdCustomerResponse.CustomerId.Should().Be(command.Id);
        createdCustomerResponse.IdentityUserId.Should().Be(fakeIdentityUser.Id);

        var createdCustomer = await SharedFixture.ExecuteEfDbContextAsync(async db =>
            await db.Customers.SingleOrDefaultAsync(x => x.Id == createdCustomerResponse.CustomerId));

        createdCustomer.Should().NotBeNull();
        createdCustomer!.IdentityId.Should().Be(fakeIdentityUser.Id);
        createdCustomer.Email.Value.Should().Be(fakeIdentityUser.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_throw_exception_when_identity_user_with_customer_email_not_exists()
    {
        // Arrange
        var command = new CreateCustomer("test@example.com");

        // Act
        Func<Task> act = async () => await SharedFixture.SendAsync(command);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<HttpResponseException>()
            .Where(x => x.StatusCode == HttpStatusCode.NotFound &&
                        !string.IsNullOrWhiteSpace(x.Message));
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_throw_exception_when_customer_with_email_already_exists()
    {
        // Arrange
        var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail().Response
            .UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        await SharedFixture.SendAsync(command);

        Func<Task> act = async () =>
            await SharedFixture.SendAsync(new CreateCustomer(fakeIdentityUser.Email));

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerAlreadyExistsException>();
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_save_mongo_customer_read_model_in_internal_persistence_message()
    {
        // Arrange
        var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail().Response
            .UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        await SharedFixture.SendAsync(command);

        // Assert
        await SharedFixture.ShouldProcessedPersistInternalCommand<CreateMongoCustomerReadModels>();
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_create_new_mongo_customer_read_model_in_the_mongodb()
    {
        // Arrange
        var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail().Response
            .UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        await SharedFixture.SendAsync(command);

        // Assert
        await SharedFixture.WaitUntilConditionMet(async () =>
        {
            var existsCustomer = await SharedFixture.ExecuteMongoDbContextAsync(async ctx =>
            {
                var res = await ctx.Customers.AsQueryable().AnyAsync(x => x.Email == command.Email);

                return res;
            });

            return existsCustomer;
        });
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_publish_customer_created_integration_event_to_the_broker()
    {
        // Arrange
        var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail().Response
            .UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        var _ = await SharedFixture.SendAsync(command);

        // Assert
        await SharedFixture.WaitForPublishing<CustomerCreatedV1>();
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_save_customer_created_integration_event_in_the_outbox()
    {
        // Arrange
        var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock.SetupGetUserByEmail().Response
            .UserIdentity;
        var command = new CreateCustomer(fakeIdentityUser!.Email);

        // Act
        var _ = await SharedFixture.SendAsync(command);

        // Assert
        await SharedFixture.ShouldProcessedOutboxPersistMessage<CustomerCreatedV1>();
    }
}
