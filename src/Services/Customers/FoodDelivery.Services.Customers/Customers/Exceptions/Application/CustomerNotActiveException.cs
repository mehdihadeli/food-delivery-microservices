using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Application;

internal class CustomerNotActiveException(long customerId)
    : AppException($"CustomerReadModel with ID: '{customerId}' is not active.")
{
    public long CustomerId { get; } = customerId;
}
