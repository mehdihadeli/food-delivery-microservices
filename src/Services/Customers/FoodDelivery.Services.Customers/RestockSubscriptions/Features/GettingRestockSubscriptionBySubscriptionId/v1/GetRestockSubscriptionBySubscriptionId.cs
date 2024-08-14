using AutoMapper;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;

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

internal class GetRestockSubscriptionBySubscriptionIdValidatorHandler(
    CustomersReadUnitOfWork customersReadUnitOfWork,
    IMapper mapper
) : IQueryHandler<GetRestockSubscriptionBySubscriptionId, GetRestockSubscriptionBySubscriptionIdResult>
{
    public async Task<GetRestockSubscriptionBySubscriptionIdResult> Handle(
        GetRestockSubscriptionBySubscriptionId query,
        CancellationToken cancellationToken
    )
    {
        query.NotBeNull();

        var restockSubscription = await customersReadUnitOfWork.RestockSubscriptionsRepository.FindOneAsync(
            x => x.IsDeleted == false && x.RestockSubscriptionId == query.RestockSubscriptionId,
            cancellationToken
        );

        if (restockSubscription is null)
        {
            throw new RestockSubscriptionNotFoundException(query.RestockSubscriptionId);
        }

        var subscriptionDto = mapper.Map<RestockSubscriptionDto>(restockSubscription);

        return new GetRestockSubscriptionBySubscriptionIdResult(subscriptionDto);
    }
}

public record GetRestockSubscriptionBySubscriptionIdResult(RestockSubscriptionDto RestockSubscription);
