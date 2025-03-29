using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.Read;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Catalogs.Products;

// we use mapperly for simple mappings and manual mapping for complex mappings
// https://mapperly.riok.app/docs/configuration/static-mappers/
[Mapper]
public static partial class ProductMappings
{
    internal static partial ProductDto ToProductDto(this ProductReadModel productReadModel);

    internal static partial ProductImageDto ToProductImageDto(this ProductImageReadModel productImageReadModel);

    [MapProperty(nameof(ProductImage.ProductId.Value), nameof(ProductImageReadModel.ProductId))]
    [MapProperty(nameof(ProductImage.Id.Value), nameof(ProductImageReadModel.Id))]
    [MapperIgnoreSource(nameof(ProductImage.Product))]
    [MapperIgnoreSource(nameof(ProductImage.Created))]
    [MapperIgnoreSource(nameof(ProductImage.CreatedBy))]
    internal static partial ProductImageReadModel ToProductImageReadModel(this ProductImage productImage);

    [MapProperty(nameof(ProductImage.ProductId.Value), nameof(ProductImageDto.ProductId))]
    [MapProperty(nameof(ProductImage.Id.Value), nameof(ProductImageDto.Id))]
    [MapperIgnoreSource(nameof(ProductImage.Product))]
    [MapperIgnoreSource(nameof(ProductImage.Created))]
    [MapperIgnoreSource(nameof(ProductImage.CreatedBy))]
    internal static partial ProductImageDto ToProductImageDto(this ProductImage productImage);

    // Use Mapperly attributes to map fields with different names
    [MapProperty(nameof(ProductView.ProductId), nameof(ProductViewDto.Id))]
    [MapProperty(nameof(ProductView.ProductName), nameof(ProductViewDto.Name))]
    [MapperIgnoreSource(nameof(ProductView.CategoryId))]
    [MapperIgnoreSource(nameof(ProductView.BrandId))]
    [MapperIgnoreSource(nameof(ProductView.SupplierId))]
    [MapperIgnoreSource(nameof(ProductView.BrandName))]
    public static partial ProductViewDto ToProductViewDto(this ProductView productView);

    public static partial IEnumerable<ProductViewDto> ToProductsViewDto(this IEnumerable<ProductView> productViews);
}
