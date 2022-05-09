using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Identity.Identity.Features.VerifyEmail.Exceptions;

public class VerificationTokenIsInvalidException : BadRequestException
{
    public VerificationTokenIsInvalidException(string userId) : base(
        $"verification token is invalid for userId '{userId}'.")
    {
    }
}
