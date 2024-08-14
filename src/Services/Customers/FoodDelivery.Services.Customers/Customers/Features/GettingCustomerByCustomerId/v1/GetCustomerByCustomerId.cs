using AutoMapper;
using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Contracts;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;

internal record GetCustomerByCustomerId(long CustomerId) : IQuery<GetCustomerByCustomerIdResult>
{
    public static GetCustomerByCustomerId Of(long customerId)
    {
        return new GetCustomerByCustomerIdValidator().HandleValidation(new GetCustomerByCustomerId(customerId));
    }
}

internal class GetCustomerByCustomerIdValidator : AbstractValidator<GetCustomerByCustomerId>
{
    public GetCustomerByCustomerIdValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}

// totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
// https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
internal class GetCustomerByCustomerIdHandler(ICustomersReadUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetCustomerByCustomerId, GetCustomerByCustomerIdResult>
{
    public async Task<GetCustomerByCustomerIdResult> Handle(
        GetCustomerByCustomerId query,
        CancellationToken cancellationToken
    )
    {
        query.NotBeNull();
        var customer = await unitOfWork.CustomersRepository.FindOneAsync(
            x => x.CustomerId == query.CustomerId,
            cancellationToken: cancellationToken
        );

        if (customer == null)
            throw new CustomerNotFoundException(query.CustomerId);

        var customerDto = mapper.Map<CustomerReadDto>(customer);

        return new GetCustomerByCustomerIdResult(customerDto);
    }
}

internal record GetCustomerByCustomerIdResult(CustomerReadDto Customer);
