using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Identity.Identity.Exceptions;

public class RequiresTwoFactorException : BadRequestException
{
    public RequiresTwoFactorException(string message) : base(message)
    {
    }
}
