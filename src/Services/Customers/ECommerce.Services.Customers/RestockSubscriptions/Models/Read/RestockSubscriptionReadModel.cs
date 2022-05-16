namespace ECommerce.Services.Customers.RestockSubscriptions.Models.Read;

public class RestockSubscriptionReadModel
{
    public Guid Id { get; init; }
    public long RestockSubscriptionId { get; init; }
    public long CustomerId { get; init; }
    public string CustomerName { get; init; }
    public long ProductId { get; init; }
    public string ProductName { get; init; }
    public string Email { get; init; } = null!;
    public DateTime Created { get; init; }
    public bool Processed { get; init; }
    public DateTime? ProcessedTime { get; init; }
    public bool IsDeleted { get; init; }
}
