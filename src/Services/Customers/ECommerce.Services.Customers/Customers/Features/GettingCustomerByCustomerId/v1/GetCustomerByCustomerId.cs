using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Customers.Dtos;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Shared.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;

public record GetCustomerByCustomerId(long CustomerId) : IQuery<GetCustomerByCustomerIdResponse>
{
    internal class Validator : AbstractValidator<GetCustomerByCustomerId>
    {
        public Validator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty();
        }
    }

    internal class Handler : IQueryHandler<GetCustomerByCustomerId, GetCustomerByCustomerIdResponse>
    {
        private readonly CustomersReadDbContext _customersReadDbContext;
        private readonly IMapper _mapper;

        public Handler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
        {
            _customersReadDbContext = customersReadDbContext;
            _mapper = mapper;
        }

        public async Task<GetCustomerByCustomerIdResponse> Handle(
            GetCustomerByCustomerId query,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(query, nameof(query));

            var customer = await _customersReadDbContext.Customers.AsQueryable()
                .FirstOrDefaultAsync(x => x.CustomerId == query.CustomerId, cancellationToken: cancellationToken);

            Guard.Against.NotFound(customer, new CustomerNotFoundException(query.CustomerId));

            var customerDto = _mapper.Map<CustomerReadDto>(customer);

            return new GetCustomerByCustomerIdResponse(customerDto);
        }
    }
}
