using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands.ValueObjects;
using FoodDelivery.Services.Catalogs.Categories;
using FoodDelivery.Services.Catalogs.Products.Dtos.v1;
using FoodDelivery.Services.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Services.Catalogs.Products.Models;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Shared.Data;
using FoodDelivery.Services.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our message and events (because of handling versioning in other boundaries), just primitive types
internal record ProductCreated(
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
    // Prevent duplicate validation with value-objects just in creation not for event itself
    public static ProductCreated Of(
        ProductId id,
        Name name,
        Price price,
        Stock stock,
        ProductStatus status,
        Dimensions dimensions,
        Size size,
        ProductColor color,
        CategoryId categoryId,
        SupplierId supplierId,
        BrandId brandId,
        DateTime createdAt,
        string? description = null,
        IEnumerable<ProductImageDto>? images = null
    )
    {
        return new ProductCreated(
            id,
            name,
            price,
            stock.Available,
            stock.RestockThreshold,
            stock.MaxStockThreshold,
            status.NotBeEmpty(),
            dimensions.Width,
            dimensions.Height,
            dimensions.Depth,
            size,
            color.NotBeEmpty(),
            categoryId,
            supplierId,
            brandId,
            createdAt,
            description,
            images
        );
    }
}

internal class ProductCreatedHandler : IDomainEventHandler<ProductCreated>
{
    private readonly CatalogDbContext _dbContext;

    public ProductCreatedHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        notification.NotBeNull();

        var existed = await _dbContext.ProductsView.FirstOrDefaultAsync(
            x => x.ProductId == notification.Id,
            cancellationToken
        );

        if (existed is null)
        {
            var product = await _dbContext
                .Products.Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Supplier)
                .SingleOrDefaultAsync(x => x.Id == notification.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(notification.Id);
            }

            var productView = new ProductView
            {
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
            };

            await _dbContext.Set<ProductView>().AddAsync(productView, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

// Mapping domain event to integration event in domain event handler is better from mapping in command handler (for preserving our domain rule invariants).
internal class ProductCreatedDomainEventToIntegrationMappingHandler : IDomainEventHandler<ProductCreated>
{
    public ProductCreatedDomainEventToIntegrationMappingHandler() { }

    public Task Handle(ProductCreated domainEvent, CancellationToken cancellationToken)
    {
        // 1. Mapping DomainEvent To IntegrationEvent
        // 2. Save Integration Event to Outbox
        return Task.CompletedTask;
    }
}
