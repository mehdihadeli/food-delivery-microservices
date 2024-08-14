using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

public class RegisterIdentityUserException(string error)
    : AppException(error, StatusCodes.Status500InternalServerError);
