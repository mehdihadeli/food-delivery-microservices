using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Customers.Customers.Exceptions.Application;

public class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException(string message) : base(message)
    {
    }

    public CustomerNotFoundException(long id) : base($"Customer with id '{id}' not found.")
    {
    }
}
