using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Data.UOW.Mongo;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions.v1;

internal record GetRestockSubscriptions : PageQuery<GetRestockSubscriptionsResult>
{
    public static GetRestockSubscriptions Of(
        PageRequest pageRequest,
        IEnumerable<string> emails,
        DateTime? from,
        DateTime? to
    )
    {
        var (pageNumber, pageSize, filters, sortOrder) = pageRequest;

        return new GetRestockSubscriptionsValidator().HandleValidation(
            new GetRestockSubscriptions
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder,
                Emails = emails.ToList(),
                From = from,
                To = to
            }
        );
    }

    public IList<string> Emails { get; init; } = null!;
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

internal class GetRestockSubscriptionsValidator : AbstractValidator<GetRestockSubscriptions>
{
    public GetRestockSubscriptionsValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

internal class GeRestockSubscriptionsHandler(CustomersReadUnitOfWork customersReadUnitOfWork)
    : IQueryHandler<GetRestockSubscriptions, GetRestockSubscriptionsResult>
{
    public async Task<GetRestockSubscriptionsResult> Handle(
        GetRestockSubscriptions query,
        CancellationToken cancellationToken
    )
    {
        var restockSubscriptions = await customersReadUnitOfWork.RestockSubscriptionsRepository.GetByPageFilter(
            query,
            RestockSubscriptionsModuleMapping.ProjectToRestockSubscriptionDto,
            sortExpression: x => x.Created,
            predicate: x =>
                x.IsDeleted == false
                && (query.Emails.Any() == false || query.Emails.Contains(x.Email))
                && (
                    (query.From == null && query.To == null)
                    || (query.From == null && x.Created <= query.To)
                    || (query.To == null && x.Created >= query.From)
                    || (x.Created >= query.From && x.Created <= query.To)
                ),
            cancellationToken: cancellationToken
        );

        return new GetRestockSubscriptionsResult(restockSubscriptions);
    }
}

internal record GetRestockSubscriptionsResult(IPageList<RestockSubscriptionDto> RestockSubscriptions);
