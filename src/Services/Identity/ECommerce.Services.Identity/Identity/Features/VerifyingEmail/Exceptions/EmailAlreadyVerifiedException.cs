using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail.Exceptions;

public class EmailAlreadyVerifiedException : ConflictException
{
    public EmailAlreadyVerifiedException(string email) : base($"User with email {email} already verified.")
    {
    }
}
