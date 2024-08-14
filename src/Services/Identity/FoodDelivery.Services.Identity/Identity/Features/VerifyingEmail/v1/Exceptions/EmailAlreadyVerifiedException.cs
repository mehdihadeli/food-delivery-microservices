using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;

public class EmailAlreadyVerifiedException(string email)
    : ConflictException($"User with email {email} already verified.");
