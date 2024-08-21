using BuildingBlocks.Core.Exception.Types;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Commands;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Dtos;
using FoodDelivery.Services.Customers.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers.Features.CreatingCustomer.v1;

//https://www.testwithspring.com/lesson/the-best-practices-of-nested-unit-tests/
//https://jeremydmiller.com/2022/10/24/using-context-specification-to-better-express-complicated-tests/
//{do_something}_{given_some_condition}

// totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern
// https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
public class CreateCustomerTests : CustomerServiceUnitTestBase
{
    private readonly ILogger<CreateCustomerHandler> _logger = new NullLogger<CreateCustomerHandler>();
    private readonly IIdentityApiClient _identityApiClient = Substitute.For<IIdentityApiClient>();

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_create_customer_with_valid_inputs()
    {
        // Arrange
        // we can mock `IdentityApiClient` with `nsubstitute` or we can use `wiremock` server and its endpoint setup for getting response(that is in base CustomerServiceUnitTestBase class)

        // 1) using mocker server approach in unit test for getting response from `IdentityApiClient`, and using `base` class `IdentityApiClient` property for using mock server
        // var fakeIdentityUser = CustomersServiceMockServersFixture.IdentityServiceMock
        //     .SetupGetUserByEmail()
        //     .Response.UserIdentity;

        // 2) mocking `IdentityApiClient` for unit test
        var fakeIdentityUser = new FakeUserIdentityDto().Generate();

        var user = fakeIdentityUser.ToUserIdentity();

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _identityApiClient
            .GetUserByEmailAsync(Arg.Is<string>(x => x == fakeIdentityUser!.Email), Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new FakeCreateCustomer(fakeIdentityUser!.Email).Generate();
        var handler = new CreateCustomerHandler(_identityApiClient, CustomersDbContext, _logger);

        // Act
        var createdCustomerResponse = await handler.Handle(command, CancellationToken.None);

        // Assert
        var entity = await CustomersDbContext.Customers.FindAsync(CustomerId.Of(createdCustomerResponse.CustomerId));
        entity.Should().NotBeNull();
        entity!.Email.Value.Should().Be(command.Email);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_not_found_exception_with_none_exists_user()
    {
        // Arrange
        var command = new CreateCustomer("test@test.com");
        var handler = new CreateCustomerHandler(IdentityApiClient, CustomersDbContext, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, CancellationToken.None);
        };

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should()
            .ThrowAsync<HttpResponseException>()
            .WithMessage("*")
            .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_argument_exception_with_null_command()
    {
        // Arrange
        var handler = new CreateCustomerHandler(IdentityApiClient, CustomersDbContext, _logger);

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
    public async Task must_throw_already_exist_exception_with_an_existing_customer()
    {
        // Arrange
        var existCustomer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(existCustomer);
        await CustomersDbContext.SaveChangesAsync();

        var command = new CreateCustomer(existCustomer.Email);
        var handler = new CreateCustomerHandler(IdentityApiClient, CustomersDbContext, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, CancellationToken.None);
        };

        // Assert
        await act.Should().ThrowAsync<CustomerAlreadyExistsException>();
    }
}
