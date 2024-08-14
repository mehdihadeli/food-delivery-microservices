using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingRestockThreshold.v1;

internal record RestockThresholdChanged(long ProductId, int RestockThreshold) : DomainEvent
{
    public static RestockThresholdChanged Of(long productId, int restockThreshold)
    {
        productId.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();

        return new RestockThresholdChanged(productId, restockThreshold);
    }
}
