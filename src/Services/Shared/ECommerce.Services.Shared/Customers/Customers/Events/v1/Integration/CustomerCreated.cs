using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;

public record CustomerCreated(long CustomerId) : IntegrationEvent;
