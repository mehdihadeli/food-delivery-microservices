using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingRestockThreshold.V1;

internal record RestockThresholdChanged(long ProductId, int RestockThreshold) : DomainEvent
{
    public static RestockThresholdChanged Of(long productId, int restockThreshold)
    {
        productId.NotBeNegativeOrZero();
        restockThreshold.NotBeNegativeOrZero();

        return new RestockThresholdChanged(productId, restockThreshold);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as RestockThresholdChanged);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as RestockThresholdChanged);
    }
}
