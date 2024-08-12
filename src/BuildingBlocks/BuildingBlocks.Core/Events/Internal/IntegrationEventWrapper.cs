using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Messaging;

namespace BuildingBlocks.Core.Events.Internal;

public record IntegrationEventWrapper<TDomainEventType> : IntegrationEvent
    where TDomainEventType : IDomainEvent;
