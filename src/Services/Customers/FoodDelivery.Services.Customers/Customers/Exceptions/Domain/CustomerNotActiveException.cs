using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Domain;

internal class CustomerNotActiveException : AppException
{
    public long CustomerId { get; }

    public CustomerNotActiveException(long customerId)
        : base($"CustomerReadModel with ID: '{customerId}' is not active.")
    {
        CustomerId = customerId;
    }
}
