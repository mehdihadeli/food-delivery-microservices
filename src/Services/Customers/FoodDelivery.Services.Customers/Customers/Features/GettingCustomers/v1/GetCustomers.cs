using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Shared.Contracts;
using Sieve.Services;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomers.v1;

public record GetCustomers : PageQuery<GetCustomersResult>
{
    public static GetCustomers Of(PageRequest pageRequest)
    {
        var (pageNumber, pageSize, filters, sortOrder) = pageRequest;

        return new GetCustomersValidator().HandleValidation(
            new GetCustomers
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder,
            }
        );
    }
}

public class GetCustomersValidator : AbstractValidator<GetCustomers>
{
    public GetCustomersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetCustomersHandler(ICustomersReadUnitOfWork unitOfWork, ISieveProcessor sieveProcessor)
    : IQueryHandler<GetCustomers, GetCustomersResult>
{
    public async ValueTask<GetCustomersResult> Handle(GetCustomers request, CancellationToken cancellationToken)
    {
        var customer = await unitOfWork.CustomersRepository.GetByPageFilter(
            request,
            projectionFunc: x => x.ToCustomersReadDto(),
            sortExpression: x => x.Id,
            cancellationToken: cancellationToken
        );

        return new GetCustomersResult(customer);
    }
}

public record GetCustomersResult(IPageList<CustomerReadDto> Customers);
