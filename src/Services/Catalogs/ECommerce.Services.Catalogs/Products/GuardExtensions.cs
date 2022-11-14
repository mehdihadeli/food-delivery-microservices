using Ardalis.GuardClauses;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;

namespace ECommerce.Services.Catalogs.Products;

public static class GuardExtensions
{
    public static void ExistsProduct(this IGuardClause guardClause, bool exists, long productId)
    {
        if (exists == false)
            throw new ProductCustomNotFoundException(productId);
    }
}
