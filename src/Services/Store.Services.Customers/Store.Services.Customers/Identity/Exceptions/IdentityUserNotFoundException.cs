using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Customers.Identity.Exceptions;

public class IdentityUserNotFoundException : NotFoundException
{
    public IdentityUserNotFoundException(Guid id) : base($"Identity user with id: '{id}' not found")
    {
    }

    public IdentityUserNotFoundException(string email) : base($"Identity user with email: '{email}' not found")
    {
    }
}
