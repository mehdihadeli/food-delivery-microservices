using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Customers.Customers.Exceptions.Domain;

internal class CustomerNotActiveException : AppException
{
    public long CustomerId { get; }

    public CustomerNotActiveException(long customerId) : base($"Customer with ID: '{customerId}' is not active.")
    {
        CustomerId = customerId;
    }
}
