using System.Diagnostics;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildingBlocks.Web.Extensions;

public static class HttpContextExtensions
{
    public static string? GetTraceId(this IHttpContextAccessor httpContextAccessor)
    {
        return Activity.Current?.TraceId.ToString() ?? httpContextAccessor?.HttpContext?.TraceIdentifier;
    }

    public static string GetCorrelationId(this HttpContext httpContext)
    {
        httpContext.Request.Headers.TryGetValue("Cko-Correlation-Id", out StringValues correlationId);
        return correlationId.FirstOrDefault() ?? httpContext.TraceIdentifier;
    }

    public static TResult? ExtractXQueryObjectFromHeader<TResult>(this HttpContext httpContext, string query)
        where TResult : IPageRequest, new()
    {
        var queryModel = new TResult();
        if (!(string.IsNullOrEmpty(query) || query == "{}"))
        {
            queryModel = JsonConvert.DeserializeObject<TResult>(query);
        }

        httpContext?.Response.Headers.Add(
            "x-query",
            JsonConvert.SerializeObject(
                queryModel,
                new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()}));

        return queryModel;
    }

    public static string? GetUserHostAddress(this HttpContext context)
    {
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
               context.Connection.RemoteIpAddress?.ToString();
    }
}
