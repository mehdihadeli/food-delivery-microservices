using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.V1.Exceptions;

public class ProductAlreadySubscribedException : AppException
{
    public ProductAlreadySubscribedException(long productId, string productName)
        : base(
            $"Product with InternalCommandId '{productId}' and Name '{productName}' is already subscribed",
            StatusCodes.Status409Conflict
        ) { }
}