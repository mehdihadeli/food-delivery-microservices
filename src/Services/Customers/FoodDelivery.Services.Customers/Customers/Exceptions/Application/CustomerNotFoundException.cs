using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Application;

public class CustomerNotFoundException : AppException
{
    public CustomerNotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }

    public CustomerNotFoundException(long id)
        : base($"CustomerReadModel with id '{id}' not found.", StatusCodes.Status404NotFound) { }

    public CustomerNotFoundException(Guid id)
        : base($"CustomerReadModel with id '{id}' not found.", StatusCodes.Status404NotFound) { }
}
