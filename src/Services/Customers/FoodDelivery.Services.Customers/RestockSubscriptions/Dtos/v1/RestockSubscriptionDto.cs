namespace FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;

public record RestockSubscriptionDto(
    long Id,
    long CustomerId,
    string CustomerName,
    string Email,
    long ProductId,
    string ProductName,
    DateTime Created,
    bool Processed,
    DateTime? ProcessedTime = null
);
