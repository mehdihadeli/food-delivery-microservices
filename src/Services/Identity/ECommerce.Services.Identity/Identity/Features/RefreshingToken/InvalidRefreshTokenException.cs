using BuildingBlocks.Core.Exception.Types;
using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Identity.Features.RefreshingToken;

public class InvalidRefreshTokenException : BadRequestException
{
    public InvalidRefreshTokenException(RefreshToken? refreshToken) : base($"refresh token {refreshToken?.Token} is invalid!")
    {
    }
}
