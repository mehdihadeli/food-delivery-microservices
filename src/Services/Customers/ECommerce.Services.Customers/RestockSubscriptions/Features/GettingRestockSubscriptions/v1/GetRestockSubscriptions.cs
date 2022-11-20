using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Persistence.Mongo;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

public record GetRestockSubscriptions : ListQuery<GetRestockSubscriptionsResponse>
{
    public IList<string> Emails { get; init; } = null!;
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

internal class GetRestockSubscriptionsValidator : AbstractValidator<GetRestockSubscriptions>
{
    public GetRestockSubscriptionsValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GeRestockSubscriptionsHandler : IQueryHandler<GetRestockSubscriptions, GetRestockSubscriptionsResponse>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public GeRestockSubscriptionsHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<GetRestockSubscriptionsResponse> Handle(
        GetRestockSubscriptions query,
        CancellationToken cancellationToken)
    {
        var filtering = _customersReadDbContext.RestockSubscriptions.AsQueryable()
            .ApplyFilter(query.Filters)
            .Where(x => x.IsDeleted == false)
            .Where(e => query.Emails.Any() == false || query.Emails.Contains(e.Email))
            .Where(x => (query.From == null && query.To == null) || (query.From == null && x.Created <= query.To) ||
                        (query.To == null && x.Created >= query.From) ||
                        (x.Created >= query.From && x.Created <= query.To))
            .OrderByDescending(x => x.Created);

        var restockSubscriptions =
            await filtering.ApplyPagingAsync<RestockSubscriptionReadModel, RestockSubscriptionDto>(
                _mapper.ConfigurationProvider,
                query.Page,
                query.PageSize,
                cancellationToken: cancellationToken);

        return new GetRestockSubscriptionsResponse(restockSubscriptions);
    }
}
