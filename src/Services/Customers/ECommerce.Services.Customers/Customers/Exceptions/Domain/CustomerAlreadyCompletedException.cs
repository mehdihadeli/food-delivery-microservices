using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Customers.Exceptions.Domain;

internal class CustomerAlreadyCompletedException : AppException
{
    public long CustomerId { get; }

    public CustomerAlreadyCompletedException(long customerId)
        : base($"Customer with ID: '{customerId}' already completed.")
    {
        CustomerId = customerId;
    }
}
