using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.Integration;

public record ProductStockDebited(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent;
