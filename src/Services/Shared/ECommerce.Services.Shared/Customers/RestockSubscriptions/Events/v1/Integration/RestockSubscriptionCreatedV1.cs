using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.RestockSubscriptions.Events.v1.Integration;

public record RestockSubscriptionCreatedV1(long CustomerId, string? Email) : IntegrationEvent;
