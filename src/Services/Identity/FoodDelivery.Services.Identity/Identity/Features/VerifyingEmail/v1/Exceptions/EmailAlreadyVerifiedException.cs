using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;

public class EmailAlreadyVerifiedException : ConflictException
{
    public EmailAlreadyVerifiedException(string email)
        : base($"User with email {email} already verified.") { }
}
