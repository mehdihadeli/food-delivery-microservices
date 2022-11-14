using BuildingBlocks.Core.Exception.Types;
using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Identity.Exceptions;

public class RefreshTokenCustomNotFoundException : CustomNotFoundException
{
    public RefreshTokenCustomNotFoundException(RefreshToken? refreshToken) : base("Refresh token not found.")
    {
    }
}
