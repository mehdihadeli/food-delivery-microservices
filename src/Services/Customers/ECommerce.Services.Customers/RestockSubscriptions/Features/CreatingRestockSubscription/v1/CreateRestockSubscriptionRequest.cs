namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public record CreateRestockSubscriptionRequest(long CustomerId, long ProductId, string Email);
