using Ardalis.GuardClauses;
using Store.Services.Catalogs.Products.Exceptions.Application;

namespace Store.Services.Catalogs.Products;

public static class GuardExtensions
{
    public static void ExistsProduct(this IGuardClause guardClause, bool exists, long productId)
    {
        if (exists == false)
            throw new ProductNotFoundException(productId);
    }
}
