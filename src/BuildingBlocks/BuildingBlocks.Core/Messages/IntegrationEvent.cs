namespace BuildingBlocks.Core.Messages;

using BuildingBlocks.Abstractions.Messages;

public abstract record IntegrationEvent : Message, IIntegrationEvent;
