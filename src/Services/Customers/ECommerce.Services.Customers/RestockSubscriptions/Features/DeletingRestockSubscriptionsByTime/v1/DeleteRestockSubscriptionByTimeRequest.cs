namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime.v1;

public record DeleteRestockSubscriptionByTimeRequest(DateTime? From = null, DateTime? To = null);
