namespace BuildingBlocks.Security.Jwt;

public interface ISecurityContextAccessor
{
    string UserId { get; }
    string Role { get; }
    string JwtToken { get; }
    bool IsAuthenticated { get; }
}
