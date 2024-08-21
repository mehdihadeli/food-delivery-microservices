using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Shared.Contracts;

namespace FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;

internal record CreateCustomerRead(
    long CustomerId,
    Guid IdentityId,
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    DateTime Created,
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
        DateTime created,
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
                created,
                birthDate,
                country,
                city,
                detailAddress,
                nationality
            )
        );
    }
}

internal class CreateCustomerReadValidator : AbstractValidator<CreateCustomerRead>
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

internal class CreateCustomerReadHandler(ICustomersReadUnitOfWork unitOfWork) : ICommandHandler<CreateCustomerRead>
{
    // totally we don't need to unit test our handlers according jimmy bogard blogs and videos, and we should extract our business to domain or seperated class so we don't need repository pattern for test, but for a sample I use it here
    // https://www.reddit.com/r/dotnet/comments/rxuqrb/testing_mediator_handlers/

    public async Task Handle(CreateCustomerRead command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var readModel = command.ToCustomer();

        await unitOfWork.CustomersRepository.AddAsync(readModel, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
    }
}
