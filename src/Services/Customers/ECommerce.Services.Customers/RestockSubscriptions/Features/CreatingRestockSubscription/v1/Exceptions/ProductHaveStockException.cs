using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;

public class ProductHaveStockException : AppException
{
    public ProductHaveStockException(long productId, int quantity, string name) : base(
        $@"Product with InternalCommandId '{productId}' and name '{name}' already has available stock of '{quantity}' items.")
    {
    }
}
