using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductBrand.v1.Events.Domain;

internal record ProductBrandChanged(long BrandId, long ProductId) : DomainEvent
{
    public static ProductBrandChanged Of(long brandId, long productId)
    {
        brandId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductBrandChanged(brandId, productId);
    }
}
