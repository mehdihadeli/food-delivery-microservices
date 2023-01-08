using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Customers.Exceptions.Application;

internal class CustomerAlreadyExistsException : AppException
{
    public long? CustomerId { get; }
    public Guid? IdentityId { get; }

    public CustomerAlreadyExistsException(string message) : base(message, HttpStatusCode.Conflict)
    {
    }

    public CustomerAlreadyExistsException(Guid identityId)
        : base($"Customer with IdentityId: '{identityId}' already exists.", HttpStatusCode.Conflict)
    {
        IdentityId = identityId;
    }

    public CustomerAlreadyExistsException(long customerId)
        : base($"Customer with ID: '{customerId}' already exists.", HttpStatusCode.Conflict)
    {
        CustomerId = customerId;
    }
}
