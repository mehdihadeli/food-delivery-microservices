using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Exceptions;

public class ProductHaveStockException : AppException
{
    public ProductHaveStockException(long productId, int quantity, string name) : base(
        $@"Product with Id '{productId}' and name '{name}' already has available stock of '{quantity}' items.")
    {
    }
}
