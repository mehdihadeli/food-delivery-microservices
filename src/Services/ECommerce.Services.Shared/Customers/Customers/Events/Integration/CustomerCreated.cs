using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.Customers.Events.Integration;

public record CustomerCreated(long CustomerId) : IntegrationEvent;
