using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.SerilogLogging.Extensions;

public static class HttpContextExtensions
{
    public static string? GetMetricsCurrentResourceName(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        Endpoint? endpoint = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;

        return endpoint?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName;
    }
}
