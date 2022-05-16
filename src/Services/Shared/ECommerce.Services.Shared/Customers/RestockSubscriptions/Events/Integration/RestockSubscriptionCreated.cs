using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Customers.RestockSubscriptions.Events.Integration;

public record RestockSubscriptionCreated(long CustomerId, string? Email) : IntegrationEvent;
