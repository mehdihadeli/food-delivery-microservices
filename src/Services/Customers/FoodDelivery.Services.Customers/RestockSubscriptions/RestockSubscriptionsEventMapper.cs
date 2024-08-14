using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Messaging;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Events.Domain;
using FoodDelivery.Services.Shared.Customers.RestockSubscriptions.Events.V1.Integration;

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
            RestockSubscriptionCreated e => new RestockSubscriptionCreatedV1(e.Id, e.Email),
            _ => null
        };
    }
}
