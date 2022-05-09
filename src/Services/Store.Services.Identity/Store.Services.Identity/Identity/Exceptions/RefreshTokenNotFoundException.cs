using BuildingBlocks.Core.Exception.Types;
using Store.Services.Identity.Shared.Models;

namespace Store.Services.Identity.Identity.Exceptions;

public class RefreshTokenNotFoundException : NotFoundException
{
    public RefreshTokenNotFoundException(RefreshToken? refreshToken) : base("Refresh token not found.")
    {
    }
}
