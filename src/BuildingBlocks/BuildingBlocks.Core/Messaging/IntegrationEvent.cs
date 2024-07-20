using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Core.Messaging;

public abstract record IntegrationEvent : Message, IIntegrationEvent;
