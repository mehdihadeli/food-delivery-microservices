using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Identity.Shared.Exceptions;

public class IdentityUserNotFoundException : AppException
{
    public IdentityUserNotFoundException(string emailOrUserName)
        : base($"User with email or username: '{emailOrUserName}' not found.", StatusCodes.Status404NotFound) { }

    public IdentityUserNotFoundException(Guid id)
        : base($"User with id: '{id}' not found.", StatusCodes.Status404NotFound) { }
}
