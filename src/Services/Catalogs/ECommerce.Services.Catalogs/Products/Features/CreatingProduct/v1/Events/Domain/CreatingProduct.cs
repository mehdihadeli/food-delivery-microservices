using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1.Events.Domain;

public record CreatingProduct(
    ProductId Id,
    Name Name,
    Price Price,
    Stock Stock,
    ProductStatus Status,
    Dimensions Dimensions,
    Category? Category,
    Supplier? Supplier,
    Brand? Brand,
    string? Description = null) : DomainEvent;
