using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Catalogs.Products.Events.V1.Integration;

public record ProductStockReplenishedV1(long ProductId, int NewStock, int ReplenishedQuantity) : IntegrationEvent
{
    /// <summary>
    /// ProductStockReplenishedV1 with in-line validation.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="newStock"></param>
    /// <param name="replenishedQuantity"></param>
    /// <returns></returns>
    public static ProductStockReplenishedV1 Of(long productId, int newStock, int replenishedQuantity)
    {
        productId.NotBeNegativeOrZero();
        newStock.NotBeNegativeOrZero();
        replenishedQuantity.NotBeNegativeOrZero();

        return new ProductStockReplenishedV1(productId, newStock, replenishedQuantity);
    }
}
