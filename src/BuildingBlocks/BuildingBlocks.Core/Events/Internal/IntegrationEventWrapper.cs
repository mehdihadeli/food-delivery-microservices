using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Messaging;
using MassTransit;

namespace BuildingBlocks.Core.Events.Internal;

[ExcludeFromTopology]
public record IntegrationEventWrapper<TDomainEventType> : IntegrationEvent
    where TDomainEventType : IDomainEvent;
