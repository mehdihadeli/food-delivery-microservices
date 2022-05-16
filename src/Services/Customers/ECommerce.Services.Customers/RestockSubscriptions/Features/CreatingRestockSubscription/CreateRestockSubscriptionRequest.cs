namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription;

public record CreateRestockSubscriptionRequest(long CustomerId, long ProductId, string Email);
