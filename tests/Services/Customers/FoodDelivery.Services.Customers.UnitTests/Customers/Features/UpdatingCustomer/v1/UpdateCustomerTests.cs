using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using FoodDelivery.Services.Customers.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.UpdatingCustomer.v1;

public class UpdateCustomerTests : CustomerServiceUnitTestBase
{
    private readonly ILogger<UpdateCustomerHandler> _logger;

    public UpdateCustomerTests()
    {
        _logger = new NullLogger<UpdateCustomerHandler>();
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_update_customer_with_valid_inputs()
    {
        // Arrange
        var customerToInsert = new FakeCustomer().Generate();
        await CustomersDbContext.AddAsync(customerToInsert);
        await CustomersDbContext.SaveChangesAsync();

        var command = new FakeUpdateCustomer(customerToInsert.Id).Generate();
        var handler = new UpdateCustomerHandler(CustomersDbContext, _logger);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var entity = await CustomersDbContext.Customers.FindAsync(CustomerId.Of(command.Id));
        entity.Should().NotBeNull();
        entity!.Email.Value.Should().Be(command.Email);
        entity!.PhoneNumber.Value.Should().Be(command.PhoneNumber);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_argument_exception_with_null_command()
    {
        // Arrange
        var handler = new UpdateCustomerHandler(CustomersDbContext, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(null!, CancellationToken.None);
        };

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_not_found_when_input_customer_not_exists()
    {
        // Arrange
        var customerToInsert = new FakeCustomer().Generate();

        var command = new FakeUpdateCustomer(customerToInsert.Id).Generate();
        var handler = new UpdateCustomerHandler(CustomersDbContext, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, CancellationToken.None);
        };

        // Assert
        await act.Should().ThrowAsync<CustomerNotFoundException>();
    }
}
