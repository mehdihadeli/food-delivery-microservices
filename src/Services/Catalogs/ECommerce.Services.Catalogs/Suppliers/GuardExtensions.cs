using Ardalis.GuardClauses;
using ECommerce.Services.Catalogs.Suppliers.Exceptions.Application;

namespace ECommerce.Services.Catalogs.Suppliers;

public static class GuardExtensions
{
    public static void ExistsSupplier(this IGuardClause guardClause, bool exists, long supplierId)
    {
        if (exists == false)
            throw new SupplierNotFoundException(supplierId);
    }
}
