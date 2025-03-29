using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Contracts;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomerByCustomerId.v1;

public record GetCustomerByCustomerId(long CustomerId) : IQuery<GetCustomerByCustomerIdResult>
{
    public static GetCustomerByCustomerId Of(long customerId)
    {
        return new GetCustomerByCustomerIdValidator().HandleValidation(new GetCustomerByCustomerId(customerId));
    }
}

public class GetCustomerByCustomerIdValidator : AbstractValidator<GetCustomerByCustomerId>
{
    public GetCustomerByCustomerIdValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}

// totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
// https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
public class GetCustomerByCustomerIdHandler(ICustomersReadUnitOfWork unitOfWork)
    : IQueryHandler<GetCustomerByCustomerId, GetCustomerByCustomerIdResult>
{
    public async ValueTask<GetCustomerByCustomerIdResult> Handle(
        GetCustomerByCustomerId query,
        CancellationToken cancellationToken
    )
    {
        query.NotBeNull();
        var customerReadModel = await unitOfWork.CustomersRepository.FindOneAsync(
            x => x.CustomerId == query.CustomerId,
            cancellationToken: cancellationToken
        );

        if (customerReadModel == null)
            throw new CustomerNotFoundException(query.CustomerId);

        var customerDto = customerReadModel.ToCustomerReadDto();

        return new GetCustomerByCustomerIdResult(customerDto);
    }
}

public record GetCustomerByCustomerIdResult(CustomerReadDto Customer);
