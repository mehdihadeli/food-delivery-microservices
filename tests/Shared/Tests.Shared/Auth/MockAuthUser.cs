using System.Security.Claims;

namespace Tests.Shared.Auth;

public class MockAuthUser(params Claim[] claims)
{
    public List<Claim> Claims { get; } = claims.ToList();
}
