using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Core.Messaging;

public record IntegrationEvent : Message, IIntegrationEvent;
