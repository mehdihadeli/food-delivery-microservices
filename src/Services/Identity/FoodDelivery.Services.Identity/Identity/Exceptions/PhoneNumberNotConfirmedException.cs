using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Exceptions;

public class PhoneNumberNotConfirmedException : AppException
{
    public PhoneNumberNotConfirmedException(string phone)
        : base($"The phone number '{phone}' is not confirmed yet.", StatusCodes.Status422UnprocessableEntity) { }
}
