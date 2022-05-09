using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Security.Jwt;

public class JwtService : IJwtService
{
    public JsonWebToken GenerateJwtToken(
        string userName,
        string email,
        string userId,
        bool? isVerified = null,
        string? fullName = null,
        string? refreshToken = null,
        IReadOnlyList<Claim>? usersClaims = null,
        IReadOnlyList<string>? rolesClaims = null,
        IReadOnlyList<string>? permissionsClaims = null)
    {
        return null;
    }

    public ClaimsPrincipal? ValidateToken(string token, TokenValidationParameters? tokenValidationParameters = null)
    {
        return null;
    }
}