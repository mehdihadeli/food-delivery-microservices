using System.Net;
using BuildingBlocks.Core.Exception.Types;
using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Identity.Exceptions;

public class RefreshTokenNotFoundException : AppException
{
    public RefreshTokenNotFoundException(RefreshToken? refreshToken) : base(
        "Refresh token not found.",
        HttpStatusCode.NotFound)
    {
    }
}
