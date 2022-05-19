using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionById;

public record GetRestockSubscriptionById(long Id) : IQuery<GetRestockSubscriptionByIdResult>;

internal class GetRestockSubscriptionByIdValidator : AbstractValidator<GetRestockSubscriptionById>
{
    public GetRestockSubscriptionByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal class GetRestockSubscriptionByIdHandler
    : IQueryHandler<GetRestockSubscriptionById, GetRestockSubscriptionByIdResult>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public GetRestockSubscriptionByIdHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<GetRestockSubscriptionByIdResult> Handle(
        GetRestockSubscriptionById query,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(query, nameof(query));

        var restockSubscription =
            await _customersReadDbContext.RestockSubscriptions.AsQueryable()
                .Where(x => x.IsDeleted == false)
                .SingleOrDefaultAsync(x => x.RestockSubscriptionId == query.Id, cancellationToken: cancellationToken);

        Guard.Against.NotFound(restockSubscription, new RestockSubscriptionNotFoundException(query.Id));

        var subscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

        return new GetRestockSubscriptionByIdResult(subscriptionDto);
    }
}

public record GetRestockSubscriptionByIdResult(RestockSubscriptionDto RestockSubscription);
