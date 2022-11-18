using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;

public record ProductStockReplenished(long ProductId, int NewStock, int ReplenishedQuantity) : IntegrationEvent;
