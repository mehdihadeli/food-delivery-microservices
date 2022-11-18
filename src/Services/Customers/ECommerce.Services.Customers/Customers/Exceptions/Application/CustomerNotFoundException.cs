using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Customers.Customers.Exceptions.Application;

public class CustomerNotFoundException : AppException
{
    public CustomerNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {
    }

    public CustomerNotFoundException(long id) : base($"Customer with id '{id}' not found.", HttpStatusCode.NotFound)
    {
    }

    public CustomerNotFoundException(Guid id) : base($"Customer with id '{id}' not found.", HttpStatusCode.NotFound)
    {
    }
}
