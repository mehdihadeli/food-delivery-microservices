using CloudNativeKit.Core.Events.Internal;
using CloudNativeKit.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductSupplier.v1.Events;

public record ProductSupplierChanged(long SupplierId, long ProductId) : DomainEvent
{
    public static ProductSupplierChanged Of(long supplierId, long productId)
    {
        supplierId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductSupplierChanged(supplierId, productId);
    }
}
