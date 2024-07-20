using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Abstractions.Messaging;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Events.Domain;

namespace FoodDelivery.Services.Customers.RestockSubscriptions;

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
            RestockSubscriptionCreated e
                => new Services.Shared.Customers.RestockSubscriptions.Events.v1.Integration.RestockSubscriptionCreatedV1(
                    e.Id,
                    e.Email
                ),
            _ => null
        };
    }
}
