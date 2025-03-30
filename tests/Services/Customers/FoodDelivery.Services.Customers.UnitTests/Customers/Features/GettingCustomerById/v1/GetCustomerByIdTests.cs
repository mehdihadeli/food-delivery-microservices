using System.Linq.Expressions;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Exceptions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.GettingCustomerById.v1;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models.Read;
using FoodDelivery.Services.Customers.UnitTests.Common;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.GettingCustomerById.v1;

public class GetCustomerByIdTests : CustomerServiceUnitTestBase
{
    private readonly ICustomersReadUnitOfWork _customersReadUnitOfWork;

    public GetCustomerByIdTests()
    {
        _customersReadUnitOfWork = Substitute.For<ICustomersReadUnitOfWork>();
        var customersReadRepository = Substitute.For<ICustomerReadRepository>();
        _customersReadUnitOfWork.CustomersRepository.Returns(customersReadRepository);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_get_existing_customer_with_valid_input()
    {
        // Arrange
        var customerReadModel = new FakeCustomerReadModel().Generate();
        _customersReadUnitOfWork
            .CustomersRepository.FindOneAsync(
                Arg.Is<Expression<Func<CustomerReadModel, bool>>>(exp => exp.Compile()(customerReadModel) == true),
                Arg.Any<CancellationToken>()
            )
            .Returns(customerReadModel);

        // Act
        var query = new GetCustomerById(customerReadModel.Id);
        var handler = new GetCustomerByIdHandler(_customersReadUnitOfWork);
        var res = await handler.Handle(query, CancellationToken.None);

        await _customersReadUnitOfWork
            .CustomersRepository.Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<CustomerReadModel, bool>>>(exp => exp.Compile()(customerReadModel) == true),
                Arg.Any<CancellationToken>()
            );
        res.Should().NotBeNull();
        res.Customer.Id.Should().Be(customerReadModel.Id);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var query = new GetCustomerById(invalidId);
        var handler = new GetCustomerByIdHandler(_customersReadUnitOfWork);

        // Act
        Func<Task> act = async () => _ = await handler.Handle(query, CancellationToken.None);

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();

        await _customersReadUnitOfWork
            .CustomersRepository.Received(1)
            .FindOneAsync(
                Arg.Is<Expression<Func<CustomerReadModel, bool>>>(exp =>
                    exp.Compile()(
                        new CustomerReadModel
                        {
                            Id = invalidId,
                            IdentityId = Guid.NewGuid(),
                            CustomerId = 0,
                            Email = "",
                            FirstName = "",
                            LastName = "",
                            FullName = "",
                            PhoneNumber = "",
                            Created = DateTime.Now,
                        }
                    ) == true
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
