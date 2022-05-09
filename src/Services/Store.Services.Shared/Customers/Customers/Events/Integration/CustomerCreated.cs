using BuildingBlocks.Core.Messaging;

namespace Store.Services.Shared.Customers.Customers.Events.Integration;

public record CustomerCreated(long CustomerId) : IntegrationEvent;
