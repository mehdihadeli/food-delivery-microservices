using ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public class CreateRestockSubscriptionTests : CustomerServiceIntegrationTestBase
{
    public CreateRestockSubscriptionTests(
        SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper) : base(sharedFixture, outputHelper)
    {
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task should_create_new_customer_restock_subscription_in_postgres_db()
    {
        // Arrange
        var fakeProduct = CustomersServiceMockServersFixture.CatalogsServiceMock.SetupGetProductById().Response.Product;
        var fakeCustomer = new FakeCustomer().Generate();
        await SharedFixture.InsertEfDbContextAsync(fakeCustomer);

        var command =
            new CreateRestockSubscription(fakeCustomer.Id, fakeProduct.Id, fakeCustomer.Email.Value ?? string.Empty);

        // Act
        var createdCustomerSubscriptionResponse = await SharedFixture.SendAsync(command);

        // Assert
        createdCustomerSubscriptionResponse.RestockSubscriptionId.Should().BeGreaterThan(0);
        createdCustomerSubscriptionResponse.RestockSubscriptionId.Should().Be(command.Id);

        var createdRestockSubscription = await SharedFixture.ExecuteEfDbContextAsync(async db =>
            await db.RestockSubscriptions.SingleOrDefaultAsync(x => x.Id == createdCustomerSubscriptionResponse.RestockSubscriptionId));

        createdRestockSubscription.Should().NotBeNull();
        createdRestockSubscription!.Email.Value.Should().Be(command.Email);
        createdRestockSubscription.ProductInformation.Id.Value.Should().Be(command.ProductId);
        createdRestockSubscription.CustomerId.Value.Should().Be(command.CustomerId);
    }
}
