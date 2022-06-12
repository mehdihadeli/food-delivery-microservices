using System.Security.Claims;

namespace Tests.Shared.Mocks;

public class MockAuthUser
{
    public List<Claim> Claims { get; }

    public MockAuthUser(params Claim[] claims) => Claims = claims.ToList();
}
