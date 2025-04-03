using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using WebMotions.Fake.Authentication.JwtBearer;

namespace Tests.Shared.Extensions;

public static class HttpClientExtensions
{
    public static HttpClient AddAuthClaims(this HttpClient client, params string[] roles)
    {
        dynamic data = new ExpandoObject();
        data.sub = Guid.NewGuid();
        data.role = roles;
        client.SetFakeBearerToken((object)data);

        return client;
    }

    /// <summary>
    /// Set a fake bearer token in form of a JWT form the list of claims.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static HttpClient SetFakeJwtBearerClaims(this HttpClient client, IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7));

        var jwt = tokenHandler.WriteToken(securityToken);
        return client.SetToken(FakeJwtBearerDefaults.AuthenticationScheme, jwt);
    }
}
