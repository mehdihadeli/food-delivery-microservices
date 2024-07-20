using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Read.Mongo;
using FluentValidation;

namespace FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record CustomerCreated(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid IdentityId,
    DateTime CreatedAt,
    string? Address,
    DateTime? BirthDate,
    string? Nationality
) : DomainEvent
{
    public static CustomerCreated Of(
        long id,
        string? firstName,
        string? lastName,
        string? email,
        string? phoneNumber,
        Guid identityId,
        DateTime createdAt,
        string? address,
        DateTime? birthDate,
        string? nationality
    )
    {
        return new CustomerCreatedValidator().HandleValidation(
            new CustomerCreated(
                id,
                firstName!,
                lastName!,
                email!,
                phoneNumber!,
                identityId,
                createdAt,
                address!,
                birthDate,
                nationality!
            )
        );

        // // Also if validation rules are simple we can just validate inputs explicitly
        // id.NotBeEmpty();
        // firstName.NotBeNullOrWhiteSpace();
        // lastName.NotBeNullOrWhiteSpace();
        // return new CustomerCreated(
        //     id,
        //     firstName,
        //     lastName,
        //     email,
        //     phoneNumber,
        //     identityId,
        //     address,
        //     birthDate,
        //     nationality
        // );
    }
}

internal class CustomerCreatedValidator : AbstractValidator<CustomerCreated>
{
    public CustomerCreatedValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.IdentityId).NotEmpty();
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

internal class CustomerCreatedHandler : IDomainEventHandler<CustomerCreated>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IMapper _mapper;

    public CustomerCreatedHandler(ICommandProcessor commandProcessor, IMapper mapper)
    {
        _commandProcessor = commandProcessor;
        _mapper = mapper;
    }

    public Task Handle(CustomerCreated notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();
        var mongoReadCommand = _mapper.Map<CreateCustomerRead>(notification);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        return _commandProcessor.ScheduleAsync(new IInternalCommand[] { mongoReadCommand }, cancellationToken);
    }
}
