using BuildingBlocks.Abstractions.Domain.Events.Internal;
using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Products.Features.UpdatingProduct.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
internal record ProductUpdated(
    long Id,
    string Name,
    decimal Price,
    int AvailableStock,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    ProductColor Color,
    long CategoryId,
    long SupplierId,
    long BrandId,
    DateTime CreatedAt,
    string? Description = null,
    IEnumerable<ProductImageDto>? Images = null
) : DomainEvent
{
    public static ProductUpdated Of(
        long id,
        string? name,
        decimal price,
        int availableStock,
        int restockThreshold,
        int maxStockThreshold,
        ProductStatus status,
        int width,
        int height,
        int depth,
        string size,
        ProductColor color,
        long categoryId,
        long supplierId,
        long brandId,
        DateTime createdAt,
        string? description = null,
        IEnumerable<ProductImageDto>? images = null
    )
    {
        return new ProductUpdatedValidator().HandleValidation(
            new ProductUpdated(
                id,
                name!,
                price,
                availableStock,
                restockThreshold,
                maxStockThreshold,
                status,
                width,
                height,
                depth,
                size,
                color,
                categoryId,
                supplierId,
                brandId,
                createdAt,
                description,
                images
            )
        );
    }
}

internal class ProductUpdatedValidator : AbstractValidator<ProductUpdated>
{
    public ProductUpdatedValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0).WithMessage("Id must be greater than 0");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Price).NotEmpty().GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is required.");
        RuleFor(x => x.Color).IsInEnum().WithMessage("Color is required.");
        RuleFor(x => x.AvailableStock).NotEmpty().GreaterThan(0).WithMessage("Stock must be greater than 0");
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

internal class ProductUpdatedHandler : IDomainEventHandler<ProductUpdated>
{
    private readonly CatalogDbContext _dbContext;

    public ProductUpdatedHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ProductUpdated notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        var existed = await _dbContext.ProductsView.FirstOrDefaultAsync(
            x => x.ProductId == notification.Id,
            cancellationToken
        );

        if (existed is not null)
        {
            var product = await _dbContext.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Supplier)
                .SingleOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);

            if (product == null)
                throw new ProductNotFoundException(notification.Id);

            existed.ProductId = product!.Id;
            existed.ProductName = product.Name;
            existed.CategoryId = product.CategoryId;
            existed.CategoryName = product.Category?.Name ?? string.Empty;
            existed.SupplierId = product.SupplierId;
            existed.SupplierName = product.Supplier?.Name ?? string.Empty;
            existed.BrandId = product.BrandId;
            existed.BrandName = product.Brand?.Name ?? string.Empty;

            _dbContext.Set<ProductView>().Update(existed);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
