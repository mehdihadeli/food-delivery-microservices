using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Customers.Customers.Exceptions;
using FoodDelivery.Services.Customers.Customers.Exceptions.Application;
using FoodDelivery.Services.Customers.Customers.Models;
using FoodDelivery.Services.Customers.Customers.Models.ValueObjects;
using FoodDelivery.Services.Customers.Shared.Clients.Rest.Identity;
using FoodDelivery.Services.Customers.Shared.Data;
using Microsoft.AspNetCore.HeaderPropagation;

namespace FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record CreateCustomer(string Email) : ITxCommand<CreateCustomerResult>
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

public class CreateCustomerValidator : AbstractValidator<CreateCustomer>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().WithMessage("Email address is invalid.");
    }
}

public class CreateCustomerHandler(
    IIdentityRestClient identityRestClient,
    CustomersDbContext customersDbContext,
    ILogger<CreateCustomerHandler> logger
) : ICommandHandler<CreateCustomer, CreateCustomerResult>
{
    public async ValueTask<CreateCustomerResult> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating customer");

        command.NotBeNull();

        if (customersDbContext.Customers.Any(x => x.Email.Value == command.Email))
            throw new CustomerAlreadyExistsException($"CustomerReadModel with email '{command.Email}' already exists.");

        var identityUser = await identityRestClient.GetUserByEmailAsync(command.Email, cancellationToken);
        if (identityUser is null)
            throw new NotFoundAppException($"user with email {command.Email} not found.");

        var customer = Customer.Create(
            CustomerId.Of(command.Id),
            Email.Of(identityUser!.Email),
            PhoneNumber.Of(identityUser.PhoneNumber),
            CustomerName.Of(identityUser.FirstName, identityUser.LastName),
            identityUser.Id
        );

        await customersDbContext.AddAsync(customer, cancellationToken);

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created a customer with ID: '{@CustomerId}'", customer.Id);

        return new CreateCustomerResult(customer.Id, customer.IdentityId);
    }
}

public record CreateCustomerResult(long CustomerId, Guid IdentityUserId);
