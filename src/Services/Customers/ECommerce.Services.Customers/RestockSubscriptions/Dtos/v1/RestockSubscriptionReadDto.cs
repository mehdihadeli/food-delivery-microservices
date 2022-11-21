namespace ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;

public record RestockSubscriptionReadDto
{
    public string CustomerName { get; init; } = default!;
    public string CustomerId { get; init; } = default!;
    public string ProductId { get; init; } = default!;
    public string ProductName { get; init; } = default!;
    public bool Processed { get; init; }
    public DateTime? ProcessedTime { get; init; }
}
