using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Shared.Exceptions;

public class UserCustomNotFoundException : CustomNotFoundException
{
    public UserCustomNotFoundException(string emailOrUserName) : base($"User with email or username: '{emailOrUserName}' not found.")
    {
    }

    public UserCustomNotFoundException(Guid id) : base($"User with id: '{id}' not found.")
    {
    }
}
