using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Validation.Extensions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Models;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients.Identity;
using FoodDelivery.Services.Customers.Shared.Data;
using FluentValidation;

namespace FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
internal record CreateCustomer(string Email) : ITxCreateCommand<CreateCustomerResult>
{
    public long Id { get; } = SnowFlakIdGenerator.NewId();

    /// <summary>
    /// Create a new customer with inline validation.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static CreateCustomer Of(string? email)
    {
        return new CreateCustomerValidator().HandleValidation(new CreateCustomer(email!));
    }
}

internal class CreateCustomerValidator : AbstractValidator<CreateCustomer>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
    }
}

internal class CreateCustomerHandler : ICommandHandler<CreateCustomer, CreateCustomerResult>
{
    private readonly IIdentityApiClient _identityApiClient;
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(
        IIdentityApiClient identityApiClient,
        CustomersDbContext customersDbContext,
        ILogger<CreateCustomerHandler> logger
    )
    {
        _identityApiClient = identityApiClient;
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<CreateCustomerResult> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer");

        command.NotBeNull();

        if (_customersDbContext.Customers.Any(x => x.Email.Value == command.Email))
            throw new CustomerAlreadyExistsException($"Customer with email '{command.Email}' already exists.");

        var identityUser = await _identityApiClient.GetUserByEmailAsync(command.Email, cancellationToken);

        var customer = Customer.Create(
            CustomerId.Of(command.Id),
            Email.Of(identityUser!.Email),
            PhoneNumber.Of(identityUser.PhoneNumber),
            CustomerName.Of(identityUser.FirstName, identityUser.LastName),
            identityUser.Id
        );

        await _customersDbContext.AddAsync(customer, cancellationToken);

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created a customer with ID: '{@CustomerId}'", customer.Id);

        return new CreateCustomerResult(customer.Id, customer.IdentityId);
    }
}

internal record CreateCustomerResult(long CustomerId, Guid IdentityUserId);
