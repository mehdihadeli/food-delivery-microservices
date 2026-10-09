using Squirrel.Core.Events.Internal;
using Squirrel.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingMaxThreshold.v1;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent
{
    public static MaxThresholdChanged Of(long productId, int maxThreshold)
    {
        productId.NotBeNegativeOrZero();
        maxThreshold.NotBeNegativeOrZero();

        return new MaxThresholdChanged(productId, maxThreshold);
    }
}
