using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Exceptions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Shared.Contracts;
using Mediator;

namespace FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Internal.Mongo;

public record UpdateCustomerRead(
    Guid Id,
    long CustomerId,
    Guid IdentityId,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime? BirthDate = null,
    string? Country = null,
    string? City = null,
    string? DetailAddress = null,
    string? Nationality = null
) : InternalCommand
{
    public string FullName => $"{FirstName} {LastName}";

    public static UpdateCustomerRead Of(
        Guid id,
        long customerId,
        Guid identityId,
        string? email,
        string? firstName,
        string? lastName,
        string? phoneNumber,
        DateTime? birthDate = null,
        string? country = null,
        string? city = null,
        string? detailAddress = null,
        string? nationality = null
    )
    {
        return new UpdateCustomerReadValidator().HandleValidation(
            new UpdateCustomerRead(
                id,
                customerId,
                identityId,
                email!,
                firstName!,
                lastName!,
                phoneNumber!,
                birthDate,
                country,
                city,
                detailAddress,
                nationality
            )
        );
    }
}

public class UpdateCustomerReadValidator : AbstractValidator<UpdateCustomerRead>
{
    public UpdateCustomerReadValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(p => p.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone Number is required.")
            .MinimumLength(7)
            .WithMessage("PhoneNumber must not be less than 7 characters.")
            .MaximumLength(15)
            .WithMessage("PhoneNumber must not exceed 15 characters.");
    }
}

public class UpdateCustomerReadHandler(ICustomersReadUnitOfWork customersReadUnitOfWork)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<UpdateCustomerRead>
{
    public async ValueTask<Unit> Handle(UpdateCustomerRead command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var existingCustomer = await customersReadUnitOfWork.CustomersRepository.FindOneAsync(
            x => x.CustomerId == command.CustomerId && x.Id == command.Id && x.IdentityId == command.IdentityId,
            cancellationToken
        );

        if (existingCustomer is null)
        {
            throw new CustomerNotFoundException(command.CustomerId);
        }

        var updatedCustomer = command.ToCustomerReadModel();

        await customersReadUnitOfWork.CustomersRepository.UpdateAsync(updatedCustomer, cancellationToken);

        await customersReadUnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
