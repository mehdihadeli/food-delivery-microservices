using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

public class RegisterIdentityUserException : AppException
{
    public RegisterIdentityUserException(string error)
        : base(error, StatusCodes.Status500InternalServerError) { }
}
