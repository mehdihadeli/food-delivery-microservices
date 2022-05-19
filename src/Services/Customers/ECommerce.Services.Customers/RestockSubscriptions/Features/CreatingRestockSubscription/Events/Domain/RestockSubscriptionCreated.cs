using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Shared.Extensions;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;

public record RestockSubscriptionCreated(RestockSubscription RestockSubscription) : DomainEvent
{
    public CreateMongoRestockSubscriptionReadModels ToCreateMongoRestockSubscriptionReadModels(
        long customerId,
        string customerName)
    {
        return new CreateMongoRestockSubscriptionReadModels(
            RestockSubscription.Id,
            customerId,
            customerName,
            RestockSubscription.ProductInformation.Id,
            RestockSubscription.ProductInformation.Name,
            RestockSubscription.Email.Value,
            RestockSubscription.Created,
            RestockSubscription.Processed,
            RestockSubscription.ProcessedTime);
    }
}

internal class RestockSubscriptionCreatedHandler : IDomainEventHandler<RestockSubscriptionCreated>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly CustomersDbContext _customersDbContext;

    public RestockSubscriptionCreatedHandler(ICommandProcessor commandProcessor, CustomersDbContext customersDbContext)
    {
        _commandProcessor = commandProcessor;
        _customersDbContext = customersDbContext;
    }

    public async Task Handle(RestockSubscriptionCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var customer = await _customersDbContext.FindCustomerByIdAsync(notification.RestockSubscription.CustomerId);

        Guard.Against.NotFound(
            customer,
            new CustomerNotFoundException(notification.RestockSubscription.CustomerId));

        var mongoReadCommand =
            notification.ToCreateMongoRestockSubscriptionReadModels(customer!.Id, customer.Name.FullName);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        await _commandProcessor.ScheduleAsync(new IInternalCommand[] { mongoReadCommand }, cancellationToken);
    }
}
