using BuildingBlocks.Core.Exception.Types;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.Products.Models;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs.Dtos;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Entities;
using FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Entities;
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
    private readonly ILogger<CreateRestockSubscriptionHandler> _logger;
    private readonly ICatalogApiClient _catalogApiClient;

    public CreateRestockSubscriptionTests()
    {
        _logger = new NullLogger<CreateRestockSubscriptionHandler>();
        _catalogApiClient = Substitute.For<ICatalogApiClient>();
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task can_create_restock_subscription_with_valid_inputs()
    {
        // Arrange
        var customer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(customer);
        await CustomersDbContext.SaveChangesAsync();

        var fakeProductDto = new FakeProductDto().RuleFor(x => x.AvailableStock, 0).Generate();

        var productClientDto = new GetProductByIdClientDto(fakeProductDto);
        var product = Mapper.Map<Product>(productClientDto);

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _catalogApiClient
            .GetProductByIdAsync(Arg.Is<long>(x => x == fakeProductDto!.Id), Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new CreateRestockSubscription(customer.Id, fakeProductDto.Id, customer.Email);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, _catalogApiClient, Mapper, _logger);

        // Act
        var res = await handler.Handle(command, CancellationToken.None);

        // Assert
        res.Should().NotBeNull();
        var entity = await CustomersDbContext.RestockSubscriptions.FindAsync(
            RestockSubscriptionId.Of(res.RestockSubscriptionId)
        );
        entity.Should().NotBeNull();
        entity!.Email.Value.Should().Be(command.Email);
    }

    [CategoryTrait(TestCategory.Unit)]
    [Fact]
    public async Task must_throw_argument_exception_with_null_command()
    {
        // Arrange
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, CatalogApiClient, Mapper, _logger);

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
    public async Task must_throw_not_found_exception_with_none_exists_customer()
    {
        // Arrange
        var command = new CreateRestockSubscription(CustomerId.Of(1), ProductId.Of(1), "m@test.com");
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, CatalogApiClient, Mapper, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, CancellationToken.None);
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
        await CustomersDbContext.Customers.AddAsync(customer);
        await CustomersDbContext.SaveChangesAsync();

        // Arrange
        var command = new CreateRestockSubscription(customer.Id, ProductId.Of(1), customer.Email.Value);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, CatalogApiClient, Mapper, _logger);

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
    public async Task must_throw_product_has_stock_exception_with_existing_product_stock()
    {
        var customer = new FakeCustomer().Generate();
        await CustomersDbContext.Customers.AddAsync(customer);
        await CustomersDbContext.SaveChangesAsync(CancellationToken.None);

        var fakeProductDto = new FakeProductDto().RuleFor(x => x.AvailableStock, 10).Generate();

        var productClientDto = new GetProductByIdClientDto(fakeProductDto);
        var product = Mapper.Map<Product>(productClientDto);

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _catalogApiClient
            .GetProductByIdAsync(Arg.Is<long>(x => x == fakeProductDto!.Id), Arg.Any<CancellationToken>())
            .Returns(product);

        // Arrange
        var command = new CreateRestockSubscription(customer.Id, ProductId.Of(1), customer.Email.Value);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, _catalogApiClient, Mapper, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, CancellationToken.None);
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
        await CustomersDbContext.Customers.AddAsync(customer);
        await CustomersDbContext.SaveChangesAsync(CancellationToken.None);

        var fakeProductDto = new FakeProductDto().RuleFor(x => x.AvailableStock, 0).Generate();
        var productClientDto = new GetProductByIdClientDto(fakeProductDto);
        var product = Mapper.Map<Product>(productClientDto);

        //https://nsubstitute.github.io/help/return-for-args/
        //https://nsubstitute.github.io/help/set-return-value/
        //https://nsubstitute.github.io/help/argument-matchers/
        _catalogApiClient
            .GetProductByIdAsync(Arg.Is<long>(x => x == fakeProductDto!.Id), Arg.Any<CancellationToken>())
            .Returns(product);

        var fakeRestockSubscription = new FakeRestockSubscriptions()
            .RuleFor(x => x.Email, customer.Email)
            .RuleFor(x => x.ProductInformation, f => ProductInformation.Of(ProductId.Of(1), f.Commerce.ProductName()))
            .RuleFor(x => x.Processed, false)
            .Generate();
        await CustomersDbContext.RestockSubscriptions.AddAsync(fakeRestockSubscription);
        await CustomersDbContext.SaveChangesAsync(CancellationToken.None);

        // Arrange
        var command = new CreateRestockSubscription(customer.Id, ProductId.Of(1), customer.Email.Value);
        var handler = new CreateRestockSubscriptionHandler(CustomersDbContext, _catalogApiClient, Mapper, _logger);

        //Act
        Func<Task> act = async () =>
        {
            await handler.Handle(command, CancellationToken.None);
        };

        // Assert
        //https://fluentassertions.com/exceptions/
        await act.Should().ThrowAsync<ProductAlreadySubscribedException>();
    }
}
