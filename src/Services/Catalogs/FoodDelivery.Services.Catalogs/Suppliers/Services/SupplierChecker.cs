using BuildingBlocks.Core.Extensions;
using FoodDelivery.Services.Catalogs.Shared.Contracts;
using FoodDelivery.Services.Catalogs.Shared.Extensions;
using FoodDelivery.Services.Catalogs.Suppliers.Contracts;

namespace FoodDelivery.Services.Catalogs.Suppliers.Services;

public class SupplierChecker : ISupplierChecker
{
    private readonly ICatalogDbContext _catalogDbContext;

    public SupplierChecker(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public bool SupplierExists(SupplierId supplierId)
    {
        supplierId.NotBeNull();
        var category = _catalogDbContext.FindSupplierById(supplierId);

        return category is not null;
    }
}
