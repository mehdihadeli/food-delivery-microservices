using Squirrel.Core.Events.Internal;
using Squirrel.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductPrice.v1;

public record ProductPriceChanged(decimal Price) : DomainEvent
{
    public static ProductPriceChanged Of(decimal price)
    {
        price.NotBeNegativeOrZero();

        return new ProductPriceChanged(price);
    }
}
