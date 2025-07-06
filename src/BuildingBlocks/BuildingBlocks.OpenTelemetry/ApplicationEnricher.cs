using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.Enrichment;

namespace BuildingBlocks.OpenTelemetry;

public sealed class ApplicationEnricher(IHttpContextAccessor httpContextAccessor) : ILogEnricher
{
    public void Enrich(IEnrichmentTagCollector collector)
    {
        collector.Add("MachineName", Environment.MachineName);

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            collector.Add("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);
        }
    }
}
