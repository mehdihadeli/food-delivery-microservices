using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using Squirrel.Core.Extensions;

namespace FoodDelivery.Services.Catalogs.Suppliers.Services;

public class SupplierChecker(ICatalogDbContext catalogDbContext) : ISupplierChecker
{
    public bool SupplierExists(SupplierId supplierId)
    {
        supplierId.NotBeNull();
        var category = catalogDbContext.FindSupplierById(supplierId);

        return category is not null;
    }
}
