namespace ECommerce.Services.Customers.RestockSubscriptions.Dtos;

public class RestockSubscriptionDto
{
    public long Id { get; init; }
    public long CustomerId { get; init; }
    public string Email { get; init; }
    public long ProductId { get; init; }
    public string ProductName { get; init; }
    public DateTime Created { get; init; }
    public bool Processed { get; init; }
    public DateTime? ProcessedTime { get; init; }
}
