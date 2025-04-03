using System.Linq.Expressions;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.Customers.Exceptions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models.Read;
using FoodDelivery.Services.Customers.UnitTests.Common;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.UpdatingCustomer.v1.Read;

public class UpdateCustomerTests : CustomerServiceUnitTestBase
{
    private readonly ICustomersReadUnitOfWork _customersReadUnitOfWork;

    public UpdateCustomerTests()
    {
        _customersReadUnitOfWork = Substitute.For<ICustomersReadUnitOfWork>();
        var customersReadRepository = Substitute.For<ICustomerReadRepository>();
        _customersReadUnitOfWork.CustomersRepository.Returns(customersReadRepository);
        _customersReadUnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    // totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
    // https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_update_customer_read_with_valid_inputs()
    {
        // Arrange
        var existCustomer = new FakeCustomerReadModel().Generate();
        var fakeUpdateCustomerReadCommand = new FakeUpdateCustomerRead(
            existCustomer.Id,
            existCustomer.CustomerId,
            existCustomer.IdentityId
        ).Generate();
        var updateCustomer = fakeUpdateCustomerReadCommand.ToCustomerReadModel();

        _customersReadUnitOfWork
            .CustomersRepository.FindOneAsync(
                Arg.Is<Expression<Func<CustomerReadModel, bool>>>(exp => exp.Compile()(existCustomer) == true),
                Arg.Any<CancellationToken>()
            )
            .Returns(existCustomer);

        _customersReadUnitOfWork
            .CustomersRepository.UpdateAsync(Arg.Is(updateCustomer), Arg.Any<CancellationToken>())
            .Returns(updateCustomer);
        var handler = new UpdateCustomerReadHandler(_customersReadUnitOfWork);

        // Act
        await handler.Handle(fakeUpdateCustomerReadCommand, CancellationToken.None);

        // Assert
        await _customersReadUnitOfWork
            .CustomersRepository.Received(1)
            .UpdateAsync(Arg.Is(updateCustomer), Arg.Any<CancellationToken>());
        await _customersReadUnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _customersReadUnitOfWork
            .CustomersRepository.Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<CustomerReadModel, bool>>>(exp => exp.Compile()(existCustomer) == true),
                Arg.Any<CancellationToken>()
            );
        existCustomer.Id.Should().Be(fakeUpdateCustomerReadCommand.Id);
        existCustomer.CustomerId.Should().Be(fakeUpdateCustomerReadCommand.CustomerId);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_not_found_exception_when_customer_not_exist()
    {
        // Arrange
        var fakeUpdateCustomerReadCommand = new FakeUpdateCustomerRead(Guid.NewGuid(), 230, Guid.NewGuid()).Generate();

        _customersReadUnitOfWork
            .CustomersRepository.FindOneAsync(
                Arg.Any<Expression<Func<CustomerReadModel, bool>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult<CustomerReadModel?>(null));

        var handler = new UpdateCustomerReadHandler(_customersReadUnitOfWork);

        // Act
        Func<Task> act = async () => await handler.Handle(fakeUpdateCustomerReadCommand, CancellationToken.None);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();

        await _customersReadUnitOfWork
            .CustomersRepository.Received(1)
            .FindOneAsync(Arg.Any<Expression<Func<CustomerReadModel, bool>>>(), Arg.Any<CancellationToken>());
    }
}
