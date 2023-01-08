using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.IdsGenerator;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.Models;
using ECommerce.Services.Customers.Customers.ValueObjects;
using ECommerce.Services.Customers.Shared.Clients.Identity;
using ECommerce.Services.Customers.Shared.Data;
using FluentValidation;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;

public record CreateCustomer(string Email) : ITxCreateCommand<CreateCustomerResponse>
{
    public long Id { get; init; } = SnowFlakIdGenerator.NewId();
}

internal class CreateCustomerValidator : AbstractValidator<CreateCustomer>
{
    public CreateCustomerValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is invalid.");
    }
}

internal class CreateCustomerHandler : ICommandHandler<CreateCustomer, CreateCustomerResponse>
{
    private readonly IIdentityApiClient _identityApiClient;
    private readonly CustomersDbContext _customersDbContext;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(
        IIdentityApiClient identityApiClient,
        CustomersDbContext customersDbContext,
        ILogger<CreateCustomerHandler> logger)
    {
        _identityApiClient = identityApiClient;
        _customersDbContext = customersDbContext;
        _logger = logger;
    }

    public async Task<CreateCustomerResponse> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating customer");

        Guard.Against.Null(command, nameof(command));

        if (_customersDbContext.Customers.Any(x => x.Email.Value == command.Email))
            throw new CustomerAlreadyExistsException($"Customer with email '{command.Email}' already exists.");

        var identityUser = (await _identityApiClient.GetUserByEmailAsync(command.Email, cancellationToken))
            ?.UserIdentity;

        var customer = Customer.Create(
            CustomerId.Of(command.Id),
            Email.Of(identityUser!.Email),
            PhoneNumber.Of(identityUser.PhoneNumber),
            CustomerName.Of(identityUser.FirstName, identityUser.LastName),
            identityUser.Id);

        await _customersDbContext.AddAsync(customer, cancellationToken);

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created a customer with ID: '{@CustomerId}'", customer.Id);

        return new CreateCustomerResponse(customer.Id, customer.IdentityId);
    }
}
