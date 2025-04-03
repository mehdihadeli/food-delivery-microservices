using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Application;

internal class CustomerAlreadyVerifiedException(long customerId)
    : AppException($"CustomerReadModel with InternalCommandId: '{customerId}' already verified.")
{
    public long CustomerId { get; } = customerId;
}
