using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ChangingProductBrand.Events.Domain;

internal record ProductBrandChanged(BrandId BrandId, ProductId ProductId) : DomainEvent;
