using BuildingBlocks.Core.Exception.Types;

namespace Store.Services.Identity.Identity.Exceptions;

public class PhoneNumberNotConfirmedException : BadRequestException
{
    public PhoneNumberNotConfirmedException(string phone) : base($"The phone number '{phone}' is not confirmed yet.")
    {
    }
}
