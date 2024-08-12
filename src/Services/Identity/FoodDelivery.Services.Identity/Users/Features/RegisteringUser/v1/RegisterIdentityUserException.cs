using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.V1;

public class RegisterIdentityUserException : AppException
{
    public RegisterIdentityUserException(string error)
        : base(error, StatusCodes.Status500InternalServerError) { }
}
