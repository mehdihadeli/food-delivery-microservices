using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Identity.Identity.Exceptions;

public class PasswordIsInvalidException : AppException
{
    public PasswordIsInvalidException(string message)
        : base(message, StatusCodes.Status403Forbidden) { }
}
