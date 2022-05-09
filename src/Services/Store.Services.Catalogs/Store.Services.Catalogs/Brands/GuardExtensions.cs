using Ardalis.GuardClauses;
using Store.Services.Catalogs.Brands.Exceptions.Application;

namespace Store.Services.Catalogs.Brands;

public static class GuardExtensions
{
    public static void ExistsBrand(this IGuardClause guardClause, bool exists, long brandId)
    {
        if (!exists)
            throw new BrandNotFoundException(brandId);
    }
}
