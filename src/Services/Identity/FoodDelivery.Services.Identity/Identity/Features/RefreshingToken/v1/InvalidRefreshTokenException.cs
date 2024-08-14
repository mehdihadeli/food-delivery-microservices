using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Services.Identity.Identity.Features.RefreshingToken.v1;

public class InvalidRefreshTokenException(Shared.Models.RefreshToken? refreshToken)
    : BadRequestException($"refresh token {refreshToken?.Token} is invalid!");
