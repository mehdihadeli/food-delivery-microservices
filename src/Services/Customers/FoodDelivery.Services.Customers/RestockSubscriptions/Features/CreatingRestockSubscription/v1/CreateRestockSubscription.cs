using AutoMapper;
using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Products;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Services.Customers.RestockSubscriptions.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.V1;

public record CreateRestockSubscription(long CustomerId, long ProductId, string Email)
    : ITxCreateCommand<CreateRestockSubscriptionResult>
{
    /// <summary>
    /// Create a new RestockSubscription with inline validation.
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

internal class CreateRestockSubscriptionValidator : AbstractValidator<CreateRestockSubscription>
{
    public CreateRestockSubscriptionValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

internal class CreateRestockSubscriptionHandler
    : ICommandHandler<CreateRestockSubscription, CreateRestockSubscriptionResult>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ICatalogApiClient _catalogApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateRestockSubscriptionHandler> _logger;

    public CreateRestockSubscriptionHandler(
        CustomersDbContext customersDbContext,
        ICatalogApiClient catalogApiClient,
        IMapper mapper,
        ILogger<CreateRestockSubscriptionHandler> logger
    )
    {
        _customersDbContext = customersDbContext;
        _catalogApiClient = catalogApiClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreateRestockSubscriptionResult> Handle(
        CreateRestockSubscription request,
        CancellationToken cancellationToken
    )
    {
        request.NotBeNull();

        var existsCustomer = await _customersDbContext.Customers.AnyAsync(
            x => x.Id == CustomerId.Of(request.CustomerId),
            cancellationToken: cancellationToken
        );

        if (!existsCustomer)
        {
            throw new CustomerNotFoundException(request.CustomerId);
        }

        var product = await _catalogApiClient.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product!.AvailableStock > 0)
            throw new ProductHasStockException(product.Id, product.AvailableStock, product.Name);

        var alreadySubscribed = _customersDbContext.RestockSubscriptions.Any(x =>
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

        await _customersDbContext.AddAsync(restockSubscription, cancellationToken);

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RestockSubscription with id '{@InternalCommandId}' saved successfully",
            restockSubscription.Id
        );

        var restockSubscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

        return new CreateRestockSubscriptionResult(restockSubscriptionDto.Id);
    }
}

public record CreateRestockSubscriptionResult(long RestockSubscriptionId);
