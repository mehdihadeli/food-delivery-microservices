using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingMaxThreshold.V1;

internal record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent
{
    public static MaxThresholdChanged Of(long productId, int maxThreshold)
    {
        productId.NotBeNegativeOrZero();
        maxThreshold.NotBeNegativeOrZero();

        return new MaxThresholdChanged(productId, maxThreshold);
    }
}
