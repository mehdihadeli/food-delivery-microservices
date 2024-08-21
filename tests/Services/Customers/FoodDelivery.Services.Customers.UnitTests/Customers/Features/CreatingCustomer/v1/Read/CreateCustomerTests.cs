using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;
using FoodDelivery.Services.Customers.UnitTests.Common;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.CreatingCustomer.v1.Read;

public class CreateCustomerTests : CustomerServiceUnitTestBase
{
    private readonly ICustomersReadUnitOfWork _customersReadUnitOfWork;

    public CreateCustomerTests()
    {
        _customersReadUnitOfWork = Substitute.For<ICustomersReadUnitOfWork>();
        var customersReadRepository = Substitute.For<ICustomerReadRepository>();
        _customersReadUnitOfWork.CustomersRepository.Returns(customersReadRepository);
        _customersReadUnitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
    }

    // totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
    // https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_create_customer_read_with_valid_inputs()
    {
        // Arrange
        var fakeCreateCustomerReadCommand = new FakeCreateCustomerRead().Generate();
        var insertCustomer = fakeCreateCustomerReadCommand.ToCustomer();

        _customersReadUnitOfWork
            .CustomersRepository.AddAsync(Arg.Is(insertCustomer), Arg.Any<CancellationToken>())
            .Returns(insertCustomer);
        var handler = new CreateCustomerReadHandler(_customersReadUnitOfWork);

        // Act
        await handler.Handle(fakeCreateCustomerReadCommand, CancellationToken.None);

        // Assert
        await _customersReadUnitOfWork
            .CustomersRepository.Received(1)
            .AddAsync(Arg.Is(insertCustomer), Arg.Any<CancellationToken>());
        await _customersReadUnitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
