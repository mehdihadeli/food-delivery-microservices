using BuildingBlocks.Core.Messages;

namespace FoodDelivery.Services.Shared.Customers.RestockSubscriptions.Events.Integration.v1;

public record RestockSubscriptionCreatedV1(long CustomerId, string? Email) : IntegrationEvent;
