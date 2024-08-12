using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Customers.RestockSubscriptions.Events.V1.Integration;

public record RestockSubscriptionCreatedV1(long CustomerId, string? Email) : IntegrationEvent;
