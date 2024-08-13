using BuildingBlocks.Abstractions.Commands;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FluentValidation;
using FoodDelivery.Services.Catalogs.Brands.Contracts;
using FoodDelivery.Services.Catalogs.Brands.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Categories.Contracts;
using FoodDelivery.Services.Catalogs.Categories.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.Features.GettingProductById.V1;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers.Exceptions.Application;

namespace FoodDelivery.Services.Catalogs.Products.Features.UpdatingProduct.V1;

internal record UpdateProduct(
    long Id,
    string Name,
    decimal Price,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    ProductType ProductType,
    ProductColor ProductColor,
    int Width,
    int Height,
    int Depth,
    string Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null
) : ITxCommand
{
    // Update product command with in-line validation
    public static UpdateProduct Of(
        long id,
        string? name,
        decimal price,
        int restockThreshold,
        int maxStockThreshold,
        ProductStatus status,
        ProductType productType,
        ProductColor color,
        int width,
        int height,
        int depth,
        string? size,
        long categoryId,
        long supplierId,
        long brandId,
        string? description = null
    )
    {
        return new UpdateProductValidator().HandleValidation(
            new UpdateProduct(
                id,
                name!,
                price,
                restockThreshold,
                maxStockThreshold,
                status,
                productType,
                color,
                width,
                height,
                depth,
                size!,
                categoryId,
                supplierId,
                brandId,
                description
            )
        );
    }
}

internal class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0).WithMessage("Id must be greater than 0");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Price).NotEmpty().GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is required.");
        RuleFor(x => x.MaxStockThreshold)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("MaxStockThreshold must be greater than 0");
        RuleFor(x => x.RestockThreshold)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("RestockThreshold must be greater than 0");
        RuleFor(x => x.CategoryId).NotEmpty().GreaterThan(0).WithMessage("CategoryId must be greater than 0");
        RuleFor(x => x.SupplierId).NotEmpty().GreaterThan(0).WithMessage("SupplierId must be greater than 0");
        RuleFor(x => x.BrandId).NotEmpty().GreaterThan(0).WithMessage("BrandId must be greater than 0");
    }
}

internal class UpdateProductInvalidateCache : InvalidateCacheRequest<UpdateProduct>
{
    public override IEnumerable<string> CacheKeys(UpdateProduct request)
    {
        yield return $"{Prefix}{nameof(GetProductById)}_{request.Id}";
    }
}

internal class UpdateProductCommandHandler(
    ICatalogDbContext catalogDbContext,
    ICategoryChecker categoryChecker,
    IBrandChecker brandChecker,
    ISupplierChecker supplierChecker
) : ICommandHandler<UpdateProduct>
{
    public async Task Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (
            id,
            name,
            price,
            restockThreshold,
            maxStockThreshold,
            productStatus,
            productType,
            color,
            width,
            height,
            depth,
            size,
            categoryId,
            supplierId,
            brandId,
            description
        ) = command;

        var product = await catalogDbContext.FindProductByIdAsync(ProductId.Of(id));
        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        var category = await catalogDbContext.FindCategoryAsync(CategoryId.Of(id));
        if (category is null)
            throw new CategoryNotFoundException(categoryId);

        var brand = await catalogDbContext.FindBrandAsync(BrandId.Of(brandId));
        if (brand is null)
            throw new BrandNotFoundException(brandId);

        var supplier = await catalogDbContext.FindSupplierByIdAsync(SupplierId.Of(supplierId));
        if (supplier is null)
            throw new SupplierNotFoundException(supplierId);

        product.ChangeCategory(
            async cid => await catalogDbContext.CategoryExistsAsync(cid!, cancellationToken: cancellationToken),
            CategoryId.Of(categoryId)
        );
        product.ChangeBrand(brandChecker, BrandId.Of(brandId));
        product.ChangeSupplier(supplierChecker, SupplierId.Of(supplierId));

        product.ChangeProductDetail(
            Name.Of(name),
            productStatus,
            productType,
            Dimensions.Of(width, height, depth),
            Size.Of(size),
            color,
            null,
            description
        );
        product.ChangePrice(Price.Of(price));
        product.ChangeMaxStockThreshold(maxStockThreshold);
        product.ChangeRestockThreshold(restockThreshold);

        await catalogDbContext.SaveChangesAsync(cancellationToken);
    }
}
