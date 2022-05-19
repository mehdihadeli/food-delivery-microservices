using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.Events.Domain;

public record ProductStockReplenished(ProductId ProductId, Stock NewStock, int ReplenishedQuantity) : DomainEvent;
