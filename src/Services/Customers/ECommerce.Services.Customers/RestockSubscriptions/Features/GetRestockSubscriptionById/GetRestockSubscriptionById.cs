using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.RestockSubscriptions.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GetRestockSubscriptionById;

public record GetRestockSubscriptionById
    (Guid Id) : IQuery<GetRestockSubscriptionByIdResponse>
{
    internal class Validator : AbstractValidator<GetRestockSubscriptionById>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }

        internal class Handler : IQueryHandler<GetRestockSubscriptionById,
            GetRestockSubscriptionByIdResponse>
        {
            private readonly CustomersReadDbContext _customersReadDbContext;
            private readonly IMapper _mapper;

            public Handler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
            {
                _customersReadDbContext = customersReadDbContext;
                _mapper = mapper;
            }

            public async Task<GetRestockSubscriptionByIdResponse> Handle(
                GetRestockSubscriptionById query,
                CancellationToken cancellationToken)
            {
                Guard.Against.Null(query, nameof(query));

                var restockSubscription =
                    await _customersReadDbContext.RestockSubscriptions.AsQueryable()
                        .Where(x => x.IsDeleted == false)
                        .SingleOrDefaultAsync(
                            x => x.Id == query.Id,
                            cancellationToken);

                Guard.Against.NotFound(
                    restockSubscription,
                    new RestockSubscriptionCustomNotFoundException(query.Id));

                var subscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

                return new GetRestockSubscriptionByIdResponse(subscriptionDto);
            }
        }
    }
}
