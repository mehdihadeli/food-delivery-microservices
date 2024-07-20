using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Products.ValueObjects;
using FoodDelivery.Services.Catalogs.Suppliers;

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
