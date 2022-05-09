using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Products.ValueObjects;

namespace Store.Services.Catalogs.Products.Features.ReplenishingProductStock.Events.Domain;

public record ProductStockReplenished(ProductId ProductId, Stock NewStock, int ReplenishedQuantity) : DomainEvent;
