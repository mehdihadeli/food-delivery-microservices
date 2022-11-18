using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Users.Features.RegisteringUser.v1;

public class RegisterIdentityUserException : BadRequestException
{
    public RegisterIdentityUserException(string error) : base(error)
    {
    }
}
