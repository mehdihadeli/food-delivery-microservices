using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Web.Extensions;

// ref: https://khalidabuhakmeh.com/adding-experimental-http-methods-to-aspnet-core

public static class HttpQueryExtensions
{
    public static IEndpointConventionBuilder MapQuery(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<Query, IResult> requestDelegate)
    {
        return endpoints.MapMethods(pattern, new[] {"QUERY"}, requestDelegate);
    }

    public static IEndpointConventionBuilder MapQuery(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        RequestDelegate requestDelegate)
    {
        return endpoints.MapMethods(pattern, new[] {"QUERY"}, requestDelegate);
    }
}
