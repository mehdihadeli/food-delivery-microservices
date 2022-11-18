using ECommerce.Services.Catalogs.Products.Models;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Requests;

public record CreateProductRequest
{
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public int RestockThreshold { get; init; }
    public int MaxStockThreshold { get; init; }
    public ProductStatus Status { get; init; } = ProductStatus.Available;
    public int Height { get; init; }
    public int Width { get; init; }
    public int Depth { get; init; }
    public string Size { get; init; } = null!;
    public ProductColor Color { get; init; }
    public long CategoryId { get; init; }
    public long SupplierId { get; init; }
    public long BrandId { get; init; }
    public string? Description { get; init; }
    public IEnumerable<CreateProductImageRequest>? Images { get; init; }
}
