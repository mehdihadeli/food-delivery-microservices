using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Security.Jwt;

public class SecurityContextAccessor(IHttpContextAccessor httpContextAccessor, ILogger<SecurityContextAccessor> logger)
    : ISecurityContextAccessor
{
    private readonly ILogger<SecurityContextAccessor> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpContextAccessor _httpContextAccessor =
        httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public string? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }
    }

    public string? JwtToken
    {
        get { return _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]; }
    }

    public bool IsAuthenticated
    {
        get
        {
            var isAuthenticated = _httpContextAccessor.HttpContext?.User?.Identities?.FirstOrDefault()?.IsAuthenticated;
            return isAuthenticated.HasValue && isAuthenticated.Value;
        }
    }

    public string Role
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return role;
        }
    }
}
