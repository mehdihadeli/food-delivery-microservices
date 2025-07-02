using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Application;

public class CustomerAlreadyExistsException : AppException
{
    public long? CustomerId { get; }
    public Guid? IdentityId { get; }

    public CustomerAlreadyExistsException(string message)
        : base(message, StatusCodes.Status409Conflict) { }

    public CustomerAlreadyExistsException(Guid identityId)
        : base($"CustomerReadModel with IdentityId: '{identityId}' already exists.", StatusCodes.Status409Conflict)
    {
        IdentityId = identityId;
    }

    public CustomerAlreadyExistsException(long customerId)
        : base($"CustomerReadModel with ID: '{customerId}' already exists.", StatusCodes.Status409Conflict)
    {
        CustomerId = customerId;
    }
}
