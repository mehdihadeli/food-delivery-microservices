using Squirrel.Core.Events.Internal;
using Squirrel.Core.Extensions;

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
