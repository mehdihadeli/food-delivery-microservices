using System.Net;
using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Services.Identity.Identity.Exceptions;

public class PhoneNumberNotConfirmedException : AppException
{
    public PhoneNumberNotConfirmedException(string phone) : base(
        $"The phone number '{phone}' is not confirmed yet.",
        HttpStatusCode.UnprocessableEntity)
    {
    }
}
