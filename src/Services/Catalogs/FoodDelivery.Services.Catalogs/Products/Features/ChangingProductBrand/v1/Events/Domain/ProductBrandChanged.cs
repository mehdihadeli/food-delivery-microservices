using Squirrel.Core.Events.Internal;
using Squirrel.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductBrand.v1.Events.Domain;

public record ProductBrandChanged(long BrandId, long ProductId) : DomainEvent
{
    public static ProductBrandChanged Of(long brandId, long productId)
    {
        brandId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductBrandChanged(brandId, productId);
    }
}
