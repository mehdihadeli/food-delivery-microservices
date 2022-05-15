using BuildingBlocks.Core.Messaging;

namespace Store.Services.Shared.Customers.RestockSubscriptions.Events.Integration;

public record RestockSubscriptionCreated(long CustomerId, string? Email) : IntegrationEvent;
