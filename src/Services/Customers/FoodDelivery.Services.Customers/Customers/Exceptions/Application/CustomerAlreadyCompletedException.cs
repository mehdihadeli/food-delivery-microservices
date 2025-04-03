using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Application;

internal class CustomerAlreadyCompletedException(long customerId)
    : AppException($"CustomerReadModel with ID: '{customerId}' already completed.")
{
    public long CustomerId { get; } = customerId;
}
