using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Exceptions;

public class PhoneNumberNotConfirmedException(string phone)
    : AppException($"The phone number '{phone}' is not confirmed yet.", StatusCodes.Status422UnprocessableEntity);
