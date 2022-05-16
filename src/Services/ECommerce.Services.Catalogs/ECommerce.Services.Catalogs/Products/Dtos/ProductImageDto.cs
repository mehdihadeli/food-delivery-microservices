namespace ECommerce.Services.Catalogs.Products.Dtos;

public record ProductImageDto
{
    public long Id { get; init; }
    public string ImageUrl { get; init; } = default!;
    public bool IsMain { get; init; }
    public long ProductId { get; init; }
}
