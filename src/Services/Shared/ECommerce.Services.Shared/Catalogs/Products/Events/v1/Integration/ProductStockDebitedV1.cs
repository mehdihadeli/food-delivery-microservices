using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;

public record ProductStockDebitedV1(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent;
