using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Web.Extenions;

// ref: https://khalidabuhakmeh.com/adding-experimental-http-methods-to-aspnet-core
public static class HttpQueryExtensions
{
    public static IEndpointConventionBuilder MapQuery(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<DbLoggerCategory.Query, IResult> requestDelegate
    )
    {
        return endpoints.MapMethods(pattern, new[] { "QUERY" }, requestDelegate);
    }

    public static IEndpointConventionBuilder MapQuery(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        RequestDelegate requestDelegate
    )
    {
        return endpoints.MapMethods(pattern, new[] { "QUERY" }, requestDelegate);
    }
}
