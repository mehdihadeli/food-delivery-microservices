using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.IdsGenerator;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.Models.Read;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;

namespace FoodDelivery.Services.Catalogs.Products;

internal static class ManualProductMappings
{
    internal static Product ToProduct(
        this CreateProduct command,
        ICategoryChecker categoryChecker,
        IBrandChecker brandChecker,
        ISupplierChecker supplierChecker
    )
    {
        var (
            name,
            price,
            stock,
            status,
            type,
            dimensions,
            size,
            color,
            categoryId,
            supplierId,
            brandId,
            description,
            imageItems
        ) = command;

        var images = imageItems
            ?.Select(x => new ProductImage(
                EntityId.Of(SnowFlakIdGenerator.NewId()),
                x.ImageUrl,
                x.IsMain,
                ProductId.Of(command.Id)
            ))
            .ToList();

        // orchestration on multiple aggregate and entities in application service or handlers
        var product = Product.Create(
            command.Id,
            name,
            ProductInformation.Of(name.Value, name.Value),
            stock,
            status,
            type,
            dimensions,
            size,
            color,
            description,
            price,
            categoryId,
            supplierId,
            brandId,
            categoryChecker,
            supplierChecker,
            brandChecker,
            images
        );

        return product;
    }

    internal static ProductDto ToProductDto(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductDto(
            Id: product.Id.Value,
            Name: product.Name.Value,
            Price: product.Price.Value,
            CategoryId: product.CategoryId.Value,
            CategoryName: product.Category?.Name ?? string.Empty,
            SupplierId: product.SupplierId.Value,
            SupplierName: product.Supplier?.Name ?? string.Empty,
            BrandId: product.BrandId.Value,
            BrandName: product.Brand?.Name ?? string.Empty,
            AvailableStock: product.Stock.Available,
            RestockThreshold: product.Stock.RestockThreshold,
            MaxStockThreshold: product.Stock.MaxStockThreshold,
            ProductStatus: product.ProductStatus,
            ProductColor: product.Color,
            Size: product.Size.Value,
            Height: product.Dimensions.Height,
            Width: product.Dimensions.Width,
            Depth: product.Dimensions.Depth,
            Description: product.Description,
            Images: product.Images?.Select(x => x.ToProductImageDto()).ToList()
        );
    }

    internal static ProductReadModel ToProductReadModel(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        return new ProductReadModel
        {
            Id = product.Id.Value,
            Name = product.Name.Value,
            Description = product.Description,
            Price = product.Price.Value,
            CategoryId = product.CategoryId.Value,
            CategoryName = product.Category?.Name ?? string.Empty,
            SupplierId = product.SupplierId.Value,
            SupplierName = product.Supplier?.Name ?? string.Empty,
            BrandId = product.BrandId.Value,
            BrandName = product.Brand?.Name ?? string.Empty,
            AvailableStock = product.Stock.Available,
            RestockThreshold = product.Stock.RestockThreshold,
            MaxStockThreshold = product.Stock.MaxStockThreshold,
            ProductStatus = product.ProductStatus,
            ProductColor = product.Color,
            Size = product.Size.Value,
            Height = product.Dimensions.Height,
            Width = product.Dimensions.Width,
            Depth = product.Dimensions.Depth,
            Images = product.Images?.Select(x => x.ToProductImageReadModel()).ToList(),
        };
    }

    public static IQueryable<ProductDto> ToProductsDto(this IQueryable<Product> products)
    {
        return products.Select(product => new ProductDto(
            product.Id.Value,
            product.Name,
            product.Price,
            product.CategoryId,
            product.Category.Name,
            product.SupplierId,
            product.Supplier.Name,
            product.BrandId,
            product.Brand.Name,
            product.Stock.Available,
            product.Stock.RestockThreshold,
            product.Stock.MaxStockThreshold,
            product.ProductStatus,
            product.Color,
            product.Size,
            product.Dimensions.Height,
            product.Dimensions.Width,
            product.Dimensions.Depth,
            product.Description,
            product.Images.Select(x => x.ToProductImageDto())
        ));
    }
}
