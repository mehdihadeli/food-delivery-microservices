using BuildingBlocks.Core.Messaging;

namespace Store.Services.Shared.Catalogs.Products.Events.Integration;

public record ProductStockReplenished(long ProductId, int NewStock, int ReplenishedQuantity) : IntegrationEvent;
