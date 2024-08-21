using BuildingBlocks.Abstractions.Queries;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Contracts;

namespace FoodDelivery.Services.Customers.Customers.Features.GettingCustomerById.v1;

internal record GetCustomerById(Guid Id) : IQuery<GetCustomerByIdResult>
{
    public static GetCustomerById Of(Guid id)
    {
        return new GetCustomerByIdValidator().HandleValidation(new GetCustomerById(id));
    }
}

internal class GetCustomerByIdValidator : AbstractValidator<GetCustomerById>
{
    public GetCustomerByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

// totally we don't need to unit test our handlers according jimmy bogard blogs and videos and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
// https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/
internal class GetCustomerByIdHandler(ICustomersReadUnitOfWork unitOfWork)
    : IQueryHandler<GetCustomerById, GetCustomerByIdResult>
{
    public async Task<GetCustomerByIdResult> Handle(GetCustomerById query, CancellationToken cancellationToken)
    {
        query.NotBeNull();

        var customer = await unitOfWork.CustomersRepository.FindOneAsync(
            x => x.Id == query.Id,
            cancellationToken: cancellationToken
        );

        if (customer is null)
            throw new CustomerNotFoundException(query.Id);

        var customerDto = customer.ToCustomerReadDto();

        return new GetCustomerByIdResult(customerDto);
    }
}

internal record GetCustomerByIdResult(CustomerReadDto Customer);
