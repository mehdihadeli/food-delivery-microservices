using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Exceptions;

public class RequiresTwoFactorException : AppException
{
    public RequiresTwoFactorException(string message)
        : base(message, StatusCodes.Status404NotFound) { }
}
