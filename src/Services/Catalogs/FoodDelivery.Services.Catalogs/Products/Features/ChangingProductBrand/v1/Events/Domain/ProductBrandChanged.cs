using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Brands;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductBrand.V1.Events.Domain;

internal record ProductBrandChanged(long BrandId, long ProductId) : DomainEvent
{
    public static ProductBrandChanged Of(long brandId, long productId)
    {
        brandId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductBrandChanged(brandId, productId);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ProductBrandChanged);
    }
}
