using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;
using FluentValidation;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionBySubscriptionId.v1;

public record GetRestockSubscriptionBySubscriptionId(long RestockSubscriptionId)
    : IQuery<GetRestockSubscriptionBySubscriptionIdResult>
{
    public static GetRestockSubscriptionBySubscriptionId Of(long restockSubscriptionId)
    {
        restockSubscriptionId.NotBeNegativeOrZero();
        return new GetRestockSubscriptionBySubscriptionId(restockSubscriptionId);
    }
}

internal class GetRestockSubscriptionBySubscriptionIdValidator
    : AbstractValidator<GetRestockSubscriptionBySubscriptionId>
{
    public GetRestockSubscriptionBySubscriptionIdValidator()
    {
        RuleFor(x => x.RestockSubscriptionId).NotEmpty();
    }
}

internal class GetRestockSubscriptionBySubscriptionIdValidatorHandler
    : IQueryHandler<GetRestockSubscriptionBySubscriptionId, GetRestockSubscriptionBySubscriptionIdResult>
{
    private readonly CustomersReadUnitOfWork _customersReadUnitOfWork;
    private readonly IMapper _mapper;

    public GetRestockSubscriptionBySubscriptionIdValidatorHandler(
        CustomersReadUnitOfWork customersReadUnitOfWork,
        IMapper mapper
    )
    {
        _customersReadUnitOfWork = customersReadUnitOfWork;
        _mapper = mapper;
    }

    public async Task<GetRestockSubscriptionBySubscriptionIdResult> Handle(
        GetRestockSubscriptionBySubscriptionId query,
        CancellationToken cancellationToken
    )
    {
        query.NotBeNull();

        var restockSubscription = await _customersReadUnitOfWork.RestockSubscriptionsRepository.FindOneAsync(
            x => x.IsDeleted == false && x.RestockSubscriptionId == query.RestockSubscriptionId,
            cancellationToken
        );

        if (restockSubscription is null)
        {
            throw new RestockSubscriptionNotFoundException(query.RestockSubscriptionId);
        }

        var subscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

        return new GetRestockSubscriptionBySubscriptionIdResult(subscriptionDto);
    }
}

public record GetRestockSubscriptionBySubscriptionIdResult(RestockSubscriptionDto RestockSubscription);
