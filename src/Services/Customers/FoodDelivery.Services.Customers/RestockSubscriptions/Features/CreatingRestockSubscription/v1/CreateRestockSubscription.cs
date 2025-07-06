using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Exceptions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Models.ValueObjects;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.Products.Exceptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Catalogs;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public record CreateRestockSubscription(long CustomerId, long ProductId, string Email)
    : ITxCommand<CreateRestockSubscriptionResult>
{
    /// <summary>
    /// Create a new RestockSubscriptionReadModel with inline validation.
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="productId"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public static CreateRestockSubscription Of(long customerId, long productId, string? email)
    {
        return new CreateRestockSubscriptionValidator().HandleValidation(
            new CreateRestockSubscription(customerId, productId, email!)
        );
    }

    public long Id { get; } = SnowFlakIdGenerator.NewId();
}

public class CreateRestockSubscriptionValidator : AbstractValidator<CreateRestockSubscription>
{
    public CreateRestockSubscriptionValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CreateRestockSubscriptionHandler(
    CustomersDbContext customersDbContext,
    ICatalogsRestClient catalogsRestClient,
    ILogger<CreateRestockSubscriptionHandler> logger
) : ICommandHandler<CreateRestockSubscription, CreateRestockSubscriptionResult>
{
    public async ValueTask<CreateRestockSubscriptionResult> Handle(
        CreateRestockSubscription request,
        CancellationToken cancellationToken
    )
    {
        request.NotBeNull();

        var existsCustomer = await customersDbContext.Customers.AnyAsync(
            x => x.Id == CustomerId.Of(request.CustomerId),
            cancellationToken: cancellationToken
        );

        if (!existsCustomer)
        {
            throw new CustomerNotFoundException(request.CustomerId);
        }

        var product = await catalogsRestClient.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        if (product!.AvailableStock > 0)
            throw new ProductHasStockException(product.Id, product.AvailableStock, product.Name);

        var alreadySubscribed = customersDbContext.RestockSubscriptions.Any(x =>
            x.Email.Value == request.Email && x.ProductInformation.Id == request.ProductId && x.Processed == false
        );

        if (alreadySubscribed)
            throw new ProductAlreadySubscribedException(product.Id, product.Name);

        var restockSubscription = RestockSubscription.Create(
            RestockSubscriptionId.Of(request.Id),
            CustomerId.Of(request.CustomerId),
            ProductInformation.Of(ProductId.Of(product.Id), product.Name),
            Email.Of(request.Email)
        );

        await customersDbContext.AddAsync(restockSubscription, cancellationToken);

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "RestockSubscriptionReadModel with id '{@InternalCommandId}' saved successfully",
            restockSubscription.Id
        );

        var restockSubscriptionDto = restockSubscription.ToRestockSubscriptionDto();

        return new CreateRestockSubscriptionResult(restockSubscriptionDto.Id);
    }
}

public record CreateRestockSubscriptionResult(long RestockSubscriptionId);
