using System.Reflection;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using BuildingBlocks.Core.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Scrutor;

namespace BuildingBlocks.Web.Minimal.Extensions;

public static class MinimalApiExtensions
{
    public static IHostApplicationBuilder AddMinimalEndpoints(
        this IHostApplicationBuilder builder,
        params Assembly[] scanAssemblies
    )
    {
        if (scanAssemblies.Length == 0)
        {
            // Find assemblies that reference the current assembly
            var referencingAssemblies = Assembly.GetExecutingAssembly().GetReferencingAssemblies();
            scanAssemblies = referencingAssemblies.ToArray();
        }

        builder.Services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(
                    classes =>
                        classes
                            .AssignableTo<IMinimalEndpoint>()
                            .Where(type => type is { IsAbstract: false, IsInterface: false }),
                    publicOnly: false
                )
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IMinimalEndpoint>()
                .WithLifetime(ServiceLifetime.Scoped)
        );

        return builder;
    }

    /// <summary>
    /// Map registered minimal apis.
    /// </summary>
    /// <param name="endpoints"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapMinimalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        using var scope = endpoints.ServiceProvider.CreateScope();
        var minimalEndpoints = scope.ServiceProvider.GetServices<IMinimalEndpoint>();

        // First, group all endpoints by their category/group name
        var endpointGroups = minimalEndpoints.GroupBy(e => e.GroupName);

        foreach (var group in endpointGroups)
        {
            var groupName = group.Key;
            var versionedApi = endpoints.NewVersionedApi(name: groupName).WithTags(groupName);

            // Get all unique versions for this group and order them
            var versions = group.Select(e => e.Version).Distinct().OrderBy(v => v);

            foreach (var version in versions)
            {
                // Get the route prefix (all endpoints in this version group share the same prefix)
                var routePrefix = group.First(e => e.Version == version).PrefixRoute;

                // Create versioned subgroup
                var versionedGroup = versionedApi.MapGroup(routePrefix).HasApiVersion(version).MapToApiVersion(version);

                // Map all endpoints for this specific version
                foreach (var endpoint in group.Where(e => e.Version == version))
                {
                    endpoint.MapEndpoint(versionedGroup);
                }
            }
        }

        return endpoints;
    }
}
