using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductPrice.v1;

public record ProductPriceChanged(decimal Price) : DomainEvent
{
    public static ProductPriceChanged Of(decimal price)
    {
        price.NotBeNegativeOrZero();

        return new ProductPriceChanged(price);
    }
}
