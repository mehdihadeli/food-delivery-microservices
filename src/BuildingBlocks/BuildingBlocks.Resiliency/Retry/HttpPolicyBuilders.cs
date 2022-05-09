using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace BuildingBlocks.Resiliency.Retry;

public static class HttpPolicyBuilders
{
    public static PolicyBuilder<HttpResponseMessage> GetBaseBuilder()
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.BadRequest);
    }
}
