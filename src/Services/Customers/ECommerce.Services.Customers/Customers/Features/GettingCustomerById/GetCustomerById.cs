using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Customers.Dtos;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomerById;

public record GetCustomerById(long Id) : IQuery<GetCustomerByIdResponse>;

internal class GetCustomerByIdValidator : AbstractValidator<GetCustomerById>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal class GetRestockSubscriptionByIdHandler
    : IQueryHandler<GetCustomerById, GetCustomerByIdResponse>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public GetRestockSubscriptionByIdHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<GetCustomerByIdResponse> Handle(
        GetCustomerById query,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(query, nameof(query));

        var customer = await _customersReadDbContext.Customers.AsQueryable()
            .SingleOrDefaultAsync(x => x.CustomerId == query.Id, cancellationToken: cancellationToken);

        Guard.Against.NotFound(customer, new CustomerNotFoundException(query.Id));

        var customerDto = _mapper.Map<CustomerReadDto>(customer);

        return new GetCustomerByIdResponse(customerDto);
    }
}
