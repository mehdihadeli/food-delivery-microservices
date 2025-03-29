using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Utils;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BuildingBlocks.Security.Jwt;

public class JwtService(IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public GenerateTokenResult GenerateJwtToken(
        string userName,
        string email,
        string userId,
        bool? isVerified = null,
        string? fullName = null,
        string? refreshToken = null,
        IReadOnlyList<Claim>? usersClaims = null,
        IReadOnlyList<string>? rolesClaims = null,
        IReadOnlyList<string>? permissionsClaims = null
    )
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User ID claim (subject) cannot be empty.", nameof(userName));

        var now = DateTime.Now;
        var ipAddress = IpUtilities.GetIpAddress();

        // https://leastprivilege.com/2017/11/15/missing-claims-in-the-asp-net-core-2-openid-connect-handler/
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/a301921ff5904b2fe084c38e41c969f4b2166bcb/src/System.IdentityModel.Tokens.Jwt/ClaimTypeMapping.cs#L45-L125
        // https://stackoverflow.com/a/50012477/581476
        var jwtClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, userId),
            new(JwtRegisteredClaimNames.Name, fullName ?? ""),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Sid, userId),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.GivenName, fullName ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(
                JwtRegisteredClaimNames.Iat,
                DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)
            ),
            new(CustomClaimTypes.RefreshToken, refreshToken ?? ""),
            new(CustomClaimTypes.IpAddress, ipAddress),
        };

        if (rolesClaims?.Any() is true)
        {
            foreach (var role in rolesClaims)
                jwtClaims.Add(new Claim(ClaimTypes.Role, role.ToLower(CultureInfo.InvariantCulture)));
        }

        if (!string.IsNullOrWhiteSpace(_jwtOptions.Audience))
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Aud, _jwtOptions.Audience));

        if (permissionsClaims?.Any() is true)
        {
            foreach (var permissionsClaim in permissionsClaims)
            {
                jwtClaims.Add(
                    new Claim(CustomClaimTypes.Permission, permissionsClaim.ToLower(CultureInfo.InvariantCulture))
                );
            }
        }

        if (usersClaims?.Any() is true)
            jwtClaims = jwtClaims.Union(usersClaims).ToList();

        _jwtOptions.SecretKey.NotBeNullOrWhiteSpace();

        SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expireTime = now.AddSeconds(_jwtOptions.TokenLifeTimeSecond == 0 ? 300 : _jwtOptions.TokenLifeTimeSecond);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] { new(ClaimTypes.Name, userId) }),
            Expires = expireTime,
            SigningCredentials = signingCredentials,
            Claims = jwtClaims.ConvertClaimsToDictionary(),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Issuer,
            NotBefore = now,
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        jwtSecurityTokenHandler.OutboundClaimTypeMap.Clear();
        var securityToken = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        var token = jwtSecurityTokenHandler.WriteToken(securityToken);

        return new GenerateTokenResult(token, expireTime);
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        token.NotBeNullOrWhiteSpace();
        _jwtOptions.SecretKey.NotBeNullOrWhiteSpace();

        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        ClaimsPrincipal principal = tokenHandler.ValidateToken(
            token,
            tokenValidationParameters,
            out SecurityToken securityToken
        );

        JwtSecurityToken? jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null)
        {
            throw new SecurityTokenException("Invalid access token.");
        }

        return principal;
    }
}

public static class JwtHelper
{
    public static IDictionary<string, object> ConvertClaimsToDictionary(this IList<Claim> claims)
    {
        return claims.ToDictionary(claim => claim.Type, claim => (object)claim.Value);
    }
}
