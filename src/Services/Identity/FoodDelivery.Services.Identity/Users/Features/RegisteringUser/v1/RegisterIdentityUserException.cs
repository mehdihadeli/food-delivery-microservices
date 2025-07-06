using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Identity.Users.Features.RegisteringUser.v1;

public class RegisterIdentityUserException(string error)
    : AppException(error, StatusCodes.Status500InternalServerError);
