using BuildingBlocks.Core.Exception;

namespace FoodDelivery.Services.Identity.Identity.Exceptions;

// https://stackoverflow.com/questions/36283377/http-status-for-email-not-verified
public class EmailNotConfirmedException : AppException
{
    public EmailNotConfirmedException(string email)
        : base($"Email not confirmed for email address `{email}`", StatusCodes.Status422UnprocessableEntity)
    {
        Email = email;
    }

    public string Email { get; }
}
