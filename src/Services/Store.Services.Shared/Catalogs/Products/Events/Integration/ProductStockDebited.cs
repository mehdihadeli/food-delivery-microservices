using BuildingBlocks.Core.Messaging;

namespace Store.Services.Shared.Catalogs.Products.Events.Integration;

public record ProductStockDebited(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent;
