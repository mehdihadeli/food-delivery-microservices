using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.Customers.Events.v1.Integration;

public record CustomerCreatedV1(long CustomerId) : IntegrationEvent;
