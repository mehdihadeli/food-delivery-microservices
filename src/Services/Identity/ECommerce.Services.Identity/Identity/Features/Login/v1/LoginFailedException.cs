using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Identity.Features.Login.v1;

public class LoginFailedException : AppException
{
    public LoginFailedException(string userNameOrEmail) : base($"Login failed for username: {userNameOrEmail}")
    {
        UserNameOrEmail = userNameOrEmail;
    }

    public string UserNameOrEmail { get; }
}
