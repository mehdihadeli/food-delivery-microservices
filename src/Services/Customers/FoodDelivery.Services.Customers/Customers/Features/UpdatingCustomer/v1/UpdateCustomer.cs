using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Data;
using Mediator;
using ICommand = BuildingBlocks.Abstractions.Commands.ICommand;

namespace FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
public sealed record UpdateCustomer(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime? BirthDate = null,
    string? DetailAddress = null,
    string? Nationality = null
) : ICommand
{
    /// <summary>
    /// Update the customer with inline validation.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="email"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="birthDate"></param>
    /// <param name="detailAddress"></param>
    /// <param name="nationality"></param>
    /// <returns></returns>
    public static UpdateCustomer Of(
        long id,
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        DateTime? birthDate = null,
        string? detailAddress = null,
        string? nationality = null
    )
    {
        return new UpdateCustomerValidator().HandleValidation(
            new UpdateCustomer(id, firstName!, lastName!, email!, phoneNumber!, birthDate, detailAddress, nationality)
        );
    }
}

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomer>
{
    public UpdateCustomerValidator()
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

public class UpdateCustomerHandler(CustomersDbContext customersDbContext, ILogger<UpdateCustomerHandler> logger)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<UpdateCustomer>
{
    public async ValueTask<Unit> Handle(UpdateCustomer command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating customer");

        command.NotBeNull();

        var customer = await customersDbContext.Customers.FindAsync(
            new object?[] { CustomerId.Of(command.Id) },
            cancellationToken: cancellationToken
        );

        if (customer is null)
        {
            throw new CustomerNotFoundException(command.Id);
        }

        customer.Update(
            Email.Of(command.Email),
            PhoneNumber.Of(command.PhoneNumber),
            CustomerName.Of(command.FirstName, command.LastName),
            null,
            command.BirthDate == null ? null : BirthDate.Of((DateTime)command.BirthDate),
            command.Nationality == null ? null : Nationality.Of(command.Nationality)
        );

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("CustomerReadModel with Id: '{@CustomerId}' updated", customer.Id);

        // TODO: Update Identity user with new customer changes

        return Unit.Value;
    }
}
