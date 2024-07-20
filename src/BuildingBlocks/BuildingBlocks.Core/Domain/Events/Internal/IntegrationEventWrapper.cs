using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Messaging;

namespace BuildingBlocks.Core.Domain.Events.Internal;

public record IntegrationEventWrapper<TDomainEventType> : IntegrationEvent
    where TDomainEventType : IDomainEvent;
