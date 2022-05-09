using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Brands;
using Store.Services.Catalogs.Categories;
using Store.Services.Catalogs.Products.Models;
using Store.Services.Catalogs.Products.ValueObjects;
using Store.Services.Catalogs.Suppliers;

namespace Store.Services.Catalogs.Products.Features.CreatingProduct.Events.Domain;

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
