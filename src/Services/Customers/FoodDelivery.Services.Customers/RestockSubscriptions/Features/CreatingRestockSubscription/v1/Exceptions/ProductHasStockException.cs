using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.V1.Exceptions;

public class ProductHasStockException : AppException
{
    public ProductHasStockException(long productId, int quantity, string name)
        : base(
            $@"Product with InternalCommandId '{productId}' and name '{name}' already has available stock of '{quantity}' items."
        ) { }
}
