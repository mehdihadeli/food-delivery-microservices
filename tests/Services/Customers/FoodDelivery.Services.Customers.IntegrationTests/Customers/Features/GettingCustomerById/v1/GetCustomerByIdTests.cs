using FluentAssertions;
using FoodDelivery.Services.Customers.Api;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.GettingCustomerById.v1;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.Shared.Data;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models.Read;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.IntegrationTests.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdTests(
    SharedFixtureWithEfCoreAndMongo<CustomersApiMetadata, CustomersDbContext, CustomersReadDbContext> sharedFixture
) : CustomerServiceIntegrationTestBase(sharedFixture)
{
    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    internal async Task can_returns_valid_read_customer_model()
    {
        // Arrange
        CustomerReadModel fakeCustomerReadModel = new FakeCustomerReadModel().Generate();
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomerReadModel);

        // Act
        var query = new GetCustomerById(fakeCustomerReadModel.Id);
        var customer = (await SharedFixture.QueryAsync(query, TestContext.Current.CancellationToken)).Customer;

        // Assert
        customer.Should().BeEquivalentTo(fakeCustomerReadModel, options => options.ExcludingMissingMembers());
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    internal async Task must_throw_not_found_exception_when_item_does_not_exists_in_mongodb()
    {
        // Act
        var query = new GetCustomerById(Guid.NewGuid());
        Func<Task> act = async () => _ = await SharedFixture.QueryAsync(query);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();
    }
}
