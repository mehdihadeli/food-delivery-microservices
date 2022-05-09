using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Core.CQRS.Query;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Types;
using Store.Services.Customers.RestockSubscriptions.Dtos;
using Store.Services.Customers.RestockSubscriptions.Models.Read;
using Store.Services.Customers.Shared.Data;
using MongoDB.Driver;

namespace Store.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

public record GetRestockSubscriptions : ListQuery<GetRestockSubscriptionsResult>
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

public class GetProductsHandler : IQueryHandler<GetRestockSubscriptions, GetRestockSubscriptionsResult>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public GetProductsHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<GetRestockSubscriptionsResult> Handle(
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
            await filtering.PaginateAsync<RestockSubscriptionReadModel, RestockSubscriptionDto>(
                _mapper.ConfigurationProvider,
                query.Page,
                query.PageSize,
                cancellationToken: cancellationToken);

        return new GetRestockSubscriptionsResult(restockSubscriptions);
    }
}

public record GetRestockSubscriptionsResult(ListResultModel<RestockSubscriptionDto> RestockSubscriptions);
