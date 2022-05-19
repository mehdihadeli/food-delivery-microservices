using BuildingBlocks.Abstractions.CQRS.Events;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;

namespace ECommerce.Services.Customers.RestockSubscriptions;

public class RestockSubscriptionsEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<IIntegrationEvent?> MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToIntegrationEvent).ToList();
    }

    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            RestockSubscriptionCreated e =>
                new Services.Shared.Customers.RestockSubscriptions.Events.Integration.RestockSubscriptionCreated(
                    e.RestockSubscription.Id.Value, e.RestockSubscription.Email),
            _ => null
        };
    }
}
