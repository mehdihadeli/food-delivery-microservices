using System.Dynamic;
using System.Net;

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
}
