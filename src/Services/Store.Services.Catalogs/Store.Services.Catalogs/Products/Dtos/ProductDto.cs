using Store.Services.Catalogs.Products.Models;

namespace Store.Services.Catalogs.Products.Dtos;

public record ProductDto
{
    public long Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public long CategoryId { get; init; }
    public string CategoryName { get; init; } = default!;
    public long SupplierId { get; init; }
    public string SupplierName { get; init; } = default!;
    public long BrandId { get; init; }
    public string BrandName { get; init; } = default!;
    public int AvailableStock { get; init; }
    public int RestockThreshold { get; init; }
    public int MaxStockThreshold { get; init; }
    public ProductStatus ProductStatus { get; init; }
    public ProductColor ProductColor { get; init; }
    public string Size { get; init; }
    public int Height { get; init; }
    public int Width { get; init; }
    public int Depth { get; init; }
    public IEnumerable<ProductImageDto>? Images { get; init; }
}
