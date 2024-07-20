using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Customers.Customers.Exceptions.Application;

public class CustomerNotFoundException : AppException
{
    public CustomerNotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }

    public CustomerNotFoundException(long id)
        : base($"Customer with id '{id}' not found.", StatusCodes.Status404NotFound) { }

    public CustomerNotFoundException(Guid id)
        : base($"Customer with id '{id}' not found.", StatusCodes.Status404NotFound) { }
}
