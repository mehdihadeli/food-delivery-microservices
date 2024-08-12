using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Services.Shared.Catalogs.Products.Events.V1.Integration;

public record ProductStockDebitedV1(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent
{
    /// <summary>
    /// ProductStockDebitedV1 with in-line validation.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="newStock"></param>
    /// <param name="debitedQuantity"></param>
    /// <returns></returns>
    public static ProductStockDebitedV1 Of(long productId, int newStock, int debitedQuantity)
    {
        productId.NotBeNegativeOrZero();
        newStock.NotBeNegativeOrZero();
        debitedQuantity.NotBeNegativeOrZero();

        return new ProductStockDebitedV1(productId, newStock, debitedQuantity);
    }
}
