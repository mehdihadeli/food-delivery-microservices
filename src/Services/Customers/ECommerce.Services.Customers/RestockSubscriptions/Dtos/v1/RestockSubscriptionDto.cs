namespace ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;

public record RestockSubscriptionDto
{
    public long Id { get; init; }
    public long CustomerId { get; init; }
    public string Email { get; init; } = default!;
    public long ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public DateTime Created { get; init; }
    public bool Processed { get; init; }
    public DateTime? ProcessedTime { get; init; }
}
