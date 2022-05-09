using System.Reflection;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Web.Extensions;

public static class MinimalApiExtensions
{
    /// <summary>
    /// Automatically discover minimal endpoints definitions from assemblies
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="scanAssemblies"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder builder,
        params Assembly[] scanAssemblies)
    {
        var assemblies = scanAssemblies.Any() ? scanAssemblies : AppDomain.CurrentDomain.GetAssemblies();

        var endpoints = assemblies.SelectMany(x => x.GetTypes()).Where(t =>
            t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface
            && t.GetConstructor(Type.EmptyTypes) != null
            && typeof(IMinimalEndpointDefinition).IsAssignableFrom(t)).ToList();

        foreach (var endpoint in endpoints)
        {
            var instantiatedType = (IMinimalEndpointDefinition)Activator.CreateInstance(endpoint)!;
            instantiatedType.MapEndpoint(builder);
        }

        return builder;
    }
}
