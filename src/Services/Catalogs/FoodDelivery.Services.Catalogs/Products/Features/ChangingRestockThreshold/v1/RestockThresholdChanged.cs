using CloudNativeKit.Core.Events.Internal;
using CloudNativeKit.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingRestockThreshold.v1;

public record RestockThresholdChanged(long ProductId, int RestockThreshold) : DomainEvent
{
    public static RestockThresholdChanged Of(long productId, int restockThreshold)
    {
        productId.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();

        return new RestockThresholdChanged(productId, restockThreshold);
    }
}
