using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductCategory.v1.Events;

internal record ProductCategoryChanged(long CategoryId, long ProductId) : DomainEvent
{
    public static ProductCategoryChanged Of(long categoryId, long productId)
    {
        categoryId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductCategoryChanged(categoryId, productId);
    }
}
