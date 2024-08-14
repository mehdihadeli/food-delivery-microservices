using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Products.Features.ChangingProductSupplier.v1.Events;

internal record ProductSupplierChanged(long SupplierId, long ProductId) : DomainEvent
{
    public static ProductSupplierChanged Of(long supplierId, long productId)
    {
        supplierId.NotBeNegativeOrZero();
        productId.NotBeNegativeOrZero();

        return new ProductSupplierChanged(supplierId, productId);
    }
}
