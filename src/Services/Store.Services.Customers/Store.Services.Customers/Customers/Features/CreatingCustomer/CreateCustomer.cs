using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.IdsGenerator;
using Store.Services.Customers.Customers.Exceptions.Application;
using Store.Services.Customers.Customers.Models;
using Store.Services.Customers.Shared.Clients.Identity;
using Store.Services.Customers.Shared.Data;

namespace Store.Services.Customers.Customers.Features.CreatingCustomer;

public record CreateCustomer(string Email) : ITxCreateCommand<CreateCustomerResult>
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

public class CreateCustomerHandler : ICommandHandler<CreateCustomer, CreateCustomerResult>
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

    public async Task<CreateCustomerResult> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        if (_customersDbContext.Customers.Any(x => x.Email == command.Email))
            throw new CustomerAlreadyExistsException($"Customer with email '{command.Email}' already exists.");

        var identityUser = (await _identityApiClient.GetUserByEmailAsync(command.Email, cancellationToken))
            ?.UserIdentity;

        Guard.Against.NotFound(
            identityUser,
            new CustomerNotFoundException($"Identity user with email '{command.Email}' not found."));

        var customer = Customer.Create(
            command.Id,
            Email.Create(identityUser!.Email),
            CustomerName.Create(identityUser.FirstName, identityUser.LastName),
            identityUser.Id);

        await _customersDbContext.AddAsync(customer, cancellationToken);

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created a customer with ID: '{@CustomerId}'", customer.Id);

        return new CreateCustomerResult(
            customer.Id,
            customer.Email!,
            customer.Name.FirstName,
            customer.Name.LastName,
            customer.IdentityId);
    }
}

public record CreateCustomerResult(
    long CustomerId,
    string Email,
    string FirstName,
    string LastName,
    Guid IdentityUserId);
