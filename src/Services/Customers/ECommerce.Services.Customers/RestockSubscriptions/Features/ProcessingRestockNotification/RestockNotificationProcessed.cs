using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;

public record RestockNotificationProcessed(RestockSubscription RestockSubscription) : DomainEvent;

internal class RestockNotificationProcessedHandler : IDomainEventHandler<RestockNotificationProcessed>
{
    private readonly ICommandProcessor _commandProcessor;

    public RestockNotificationProcessedHandler(ICommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task Handle(RestockNotificationProcessed notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandProcessor.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(notification.RestockSubscription, false),
            cancellationToken);
    }
}
