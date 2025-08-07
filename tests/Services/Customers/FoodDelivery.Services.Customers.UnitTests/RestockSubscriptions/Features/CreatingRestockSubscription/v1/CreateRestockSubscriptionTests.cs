using BuildingBlocks.Core.Exception;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Models.ValueObjects;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.Products.Exceptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs.Dtos;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models;
using FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Models;
using FoodDelivery.Services.Customers.TestShared.Fakes.Shared.Dtos;
using FoodDelivery.Services.Customers.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public class CreateRestockSubscriptionTests : CustomerServiceUnitTestBase
{
    private readonly ILogger<CreateRestockSubscriptionHandler> _logger =
        new NullLogger<CreateRestockSubscriptionHandler>();
    private readonly ICatalogsRestClient _iICatalogsClient = Substitute.For<ICatalogsRestClient>();

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_create_restock_subscription_with_valid_inputs()
    {
        // Arrange
        var customer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(customer, TestContext.Current.CancellationToken);
        await CustomersDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var fakeProductDto = new FakeProductDto().RuleFor(x => x.AvailableStock, 0).Generate();

        var getProductByIdClientDto = new GetProductByIdClientDto(fakeProductDto);
        var product = getProductByIdClientDto.Product.ToProduct();

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _iICatalogsClient
            .GetProductByIdAsync(Arg.Is<long>(x => x == fakeProductDto!.Id), Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new CreateRestockSubscription(customer.Id, fakeProductDto.Id, customer.Email);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, _iICatalogsClient, _logger);

        // Act
        var res = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        res.Should().NotBeNull();
        var entity = await CustomersDbContext.RestockSubscriptions.FindAsync(
            new object?[] { RestockSubscriptionId.Of(res.RestockSubscriptionId) },
            TestContext.Current.CancellationToken
        );
        entity.Should().NotBeNull();
        entity!.Email.Value.Should().Be(command.Email);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_argument_exception_with_null_command()
    {
        // Arrange
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, IICatalogsClient, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(null!, TestContext.Current.CancellationToken);
        };

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_not_found_exception_with_none_exists_customer()
    {
        // Arrange
        var command = new CreateRestockSubscription(CustomerId.Of(1), ProductId.Of(1), "m@test.com");
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, IICatalogsClient, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        };

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<CustomerNotFoundException>();
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_not_found_exception_with_none_exists_product()
    {
        var customer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(customer, TestContext.Current.CancellationToken);
        await CustomersDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Arrange
        var command = new CreateRestockSubscription(customer.Id, ProductId.Of(1), customer.Email.Value);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, IICatalogsClient, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        };

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should()
            .ThrowAsync<ProductNotFoundException>()
            .WithMessage("*")
            .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_product_has_stock_exception_with_existing_product_stock()
    {
        var customer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(customer, TestContext.Current.CancellationToken);
        await CustomersDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var fakeProductDto = new FakeProductDto().RuleFor(x => x.AvailableStock, 10).Generate();

        var getProductByIdClientDto = new GetProductByIdClientDto(fakeProductDto);
        var product = getProductByIdClientDto.Product.ToProduct();

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _iICatalogsClient
            .GetProductByIdAsync(Arg.Is<long>(x => x == fakeProductDto!.Id), Arg.Any<CancellationToken>())
            .Returns(product);

        // Arrange
        var command = new CreateRestockSubscription(customer.Id, ProductId.Of(1), customer.Email.Value);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, _iICatalogsClient, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        };

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<ProductHasStockException>();
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_exception_when_restock_already_exists()
    {
        var customer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(customer, TestContext.Current.CancellationToken);
        await CustomersDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var fakeProductDto = new FakeProductDto().RuleFor(x => x.AvailableStock, 0).Generate();
        var getProductClientDto = new GetProductByIdClientDto(fakeProductDto);
        var product = getProductClientDto.Product.ToProduct();

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _iICatalogsClient
            .GetProductByIdAsync(Arg.Is<long>(x => x == fakeProductDto!.Id), Arg.Any<CancellationToken>())
            .Returns(product);

        var fakeRestockSubscription = new FakeRestockSubscriptions()
            .RuleFor(x => x.Email, customer.Email)
            .RuleFor(x => x.ProductInformation, f => ProductInformation.Of(ProductId.Of(1), f.Commerce.ProductName()))
            .RuleFor(x => x.Processed, false)
            .Generate();
        await CustomersDbContext.RestockSubscriptions.AddAsync(
            fakeRestockSubscription,
            TestContext.Current.CancellationToken
        );
        await CustomersDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Arrange
        var command = new CreateRestockSubscription(customer.Id, ProductId.Of(1), customer.Email.Value);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, _iICatalogsClient, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, TestContext.Current.CancellationToken);
        };

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<ProductAlreadySubscribedException>();
    }
}
