using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail.Exceptions;

public class VerificationTokenIsInvalidException : BadRequestException
{
    public VerificationTokenIsInvalidException(string userId) : base(
        $"verification token is invalid for userId '{userId}'.")
    {
    }
}
