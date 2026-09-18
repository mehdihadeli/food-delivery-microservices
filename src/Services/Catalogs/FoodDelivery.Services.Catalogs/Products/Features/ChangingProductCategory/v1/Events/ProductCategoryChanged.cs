using CloudNativeKit.Core.Events.Internal;
using CloudNativeKit.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductCategory.v1.Events;

public record ProductCategoryChanged(long CategoryId, long ProductId) : DomainEvent
{
    public static ProductCategoryChanged Of(long categoryId, long productId)
    {
        categoryId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductCategoryChanged(categoryId, productId);
    }
}
