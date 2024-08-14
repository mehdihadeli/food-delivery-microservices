using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Features.VerifyingEmail.v1.Exceptions;

public class VerificationTokenIsInvalidException(string userId)
    : BadRequestException($"verification token is invalid for userId '{userId}'.");
