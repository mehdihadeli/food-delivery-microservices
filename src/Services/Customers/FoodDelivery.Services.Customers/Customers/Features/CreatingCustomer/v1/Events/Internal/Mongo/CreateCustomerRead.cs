using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Shared.Contracts;
using Mediator;

namespace FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Internal.Mongo;

public record CreateCustomerRead(
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

    public static CreateCustomerRead Of(
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
        return new CreateCustomerReadValidator().HandleValidation(
            new CreateCustomerRead(
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

public class CreateCustomerReadValidator : AbstractValidator<CreateCustomerRead>
{
    public CreateCustomerReadValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.IdentityId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.BirthDate).NotEmpty();
        RuleFor(p => p.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone Number is required.")
            .MinimumLength(7)
            .WithMessage("PhoneNumber must not be less than 7 characters.")
            .MaximumLength(15)
            .WithMessage("PhoneNumber must not exceed 15 characters.");
    }
}

public class CreateCustomerReadHandler(ICustomersReadUnitOfWork unitOfWork)
    : BuildingBlocks.Abstractions.Commands.ICommandHandler<CreateCustomerRead>
{
    public async ValueTask<Unit> Handle(CreateCustomerRead command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var readModel = command.ToCustomerReadModel();

        await unitOfWork.CustomersRepository.AddAsync(readModel, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
