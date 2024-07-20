using FoodDelivery.Services.Catalogs.Products.Models;

namespace FoodDelivery.Services.Catalogs.Products.Dtos.v1;

public record ProductDto(
    long Id,
    string Name,
    decimal Price,
    long CategoryId,
    string CategoryName,
    long SupplierId,
    string SupplierName,
    long BrandId,
    string BrandName,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus ProductStatus,
    ProductColor ProductColor,
    string Size,
    int Height,
    int Width,
    int Depth,
    string? Description,
    IEnumerable<ProductImageDto>? Images
);
