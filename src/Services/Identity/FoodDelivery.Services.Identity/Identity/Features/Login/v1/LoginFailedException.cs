using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Features.Login.v1;

public class LoginFailedException(string userNameOrEmail)
    : AppException($"Login failed for username: {userNameOrEmail}")
{
    public string UserNameOrEmail { get; } = userNameOrEmail;
}
