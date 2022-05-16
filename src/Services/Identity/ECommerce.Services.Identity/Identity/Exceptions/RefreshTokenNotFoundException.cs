using BuildingBlocks.Core.Exception.Types;
using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Identity.Exceptions;

public class RefreshTokenNotFoundException : NotFoundException
{
    public RefreshTokenNotFoundException(RefreshToken? refreshToken) : base("Refresh token not found.")
    {
    }
}
