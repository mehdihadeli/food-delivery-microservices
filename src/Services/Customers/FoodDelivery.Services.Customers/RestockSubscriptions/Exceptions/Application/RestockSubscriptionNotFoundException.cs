using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.RestockSubscriptions.Exceptions.Application;

public class RestockSubscriptionNotFoundException : AppException
{
    public RestockSubscriptionNotFoundException(long id)
        : base($"RestockSubscriptionReadModel with id: {id}not found", StatusCodes.Status404NotFound) { }

    public RestockSubscriptionNotFoundException(Guid id)
        : base($"RestockSubscriptionReadModel with id: {id}not found", StatusCodes.Status404NotFound) { }
}
