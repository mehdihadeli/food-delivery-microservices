using FluentAssertions;
using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace FoodDelivery.Services.Customers.IntegrationTests.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public class CreateRestockSubscriptionTests(
    SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture,
    ITestOutputHelper outputHelper
) : CustomerServiceIntegrationTestBase(sharedFixture, outputHelper)
{
    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    public async Task can_create_new_customer_restock_subscription_in_postgres_db()
    {
        // Arrange
        var fakeProduct = CatalogsServiceWireMock.SetupGetProductById().Response.Product;
        var fakeCustomer = new FakeCustomer().Generate();
        await SharedFixture.InsertEfDbContextAsync(fakeCustomer);

        var command = new CreateRestockSubscription(
            fakeCustomer.Id,
            fakeProduct.Id,
            fakeCustomer.Email.Value ?? string.Empty
        );

        // Act
        var createdCustomerSubscriptionResponse = await SharedFixture.SendAsync(command);

        // Assert
        createdCustomerSubscriptionResponse.RestockSubscriptionId.Should().BeGreaterThan(0);
        createdCustomerSubscriptionResponse.RestockSubscriptionId.Should().Be(command.Id);

        var createdRestockSubscription = await SharedFixture.ExecuteEfDbContextAsync(async db =>
            await db.RestockSubscriptions.SingleOrDefaultAsync(x =>
                x.Id == createdCustomerSubscriptionResponse.RestockSubscriptionId
            )
        );

        createdRestockSubscription.Should().NotBeNull();
        createdRestockSubscription!.Email.Value.Should().Be(command.Email);
        createdRestockSubscription.ProductInformation.Id.Value.Should().Be(command.ProductId);
        createdRestockSubscription.CustomerId.Value.Should().Be(command.CustomerId);
    }
}
