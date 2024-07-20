namespace FoodDelivery.Services.Catalogs.Products.Models.Read;

public class ProductReadModel
{
    public required long Id { get; init; }
    public required string Name { get; init; } = default!;
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public required long CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required long SupplierId { get; init; }
    public required string SupplierName { get; init; }
    public required long BrandId { get; init; }
    public required string BrandName { get; init; }
    public required int AvailableStock { get; init; }
    public required int RestockThreshold { get; init; }
    public required int MaxStockThreshold { get; init; }
    public required ProductStatus ProductStatus { get; init; }
    public required ProductColor ProductColor { get; init; }
    public required string Size { get; init; }
    public required int Height { get; init; }
    public required int Width { get; init; }
    public required int Depth { get; init; }
    public IEnumerable<ProductImageReadModel>? Images { get; init; }
}
