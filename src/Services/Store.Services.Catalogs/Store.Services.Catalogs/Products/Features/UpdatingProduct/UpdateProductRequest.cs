using Store.Services.Catalogs.Products.Models;

namespace Store.Services.Catalogs.Products.Features.UpdatingProduct;

public record UpdateProductRequest
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int RestockThreshold { get; init; }
    public int MaxStockThreshold { get; init; }
    public ProductStatus Status { get; init; }
    public int Height { get; init; }
    public int Width { get; init; }
    public int Depth { get; init; }
    public string Size { get; init; } = null!;
    public long CategoryId { get; init; }
    public long SupplierId { get; init; }
    public long BrandId { get; init; }
    public string? Description { get; init; }
}
