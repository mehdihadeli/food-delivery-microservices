using AutoMapper;
using BuildingBlocks.Abstractions.Core.Paging;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.Shared.Contracts;
using FoodDelivery.Services.Customers.Shared.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sieve.Services;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomers.V1;

internal record GetCustomers : PageQuery<GetCustomersResult>
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
                SortOrder = sortOrder
            }
        );
    }
}

internal class GetCustomersValidator : AbstractValidator<GetCustomers>
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

internal class GetCustomersHandler : IQueryHandler<GetCustomers, GetCustomersResult>
{
    private readonly ICustomersReadUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;

    public GetCustomersHandler(ICustomersReadUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }

    public async Task<GetCustomersResult> Handle(GetCustomers request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.CustomersRepository.GetByPageFilter<CustomerReadDto, string>(
            request,
            _mapper.ConfigurationProvider,
            sortExpression: x => x.City!,
            cancellationToken: cancellationToken
        );

        return new GetCustomersResult(customer);
    }
}

internal record GetCustomersResult(IPageList<CustomerReadDto> Customers);
