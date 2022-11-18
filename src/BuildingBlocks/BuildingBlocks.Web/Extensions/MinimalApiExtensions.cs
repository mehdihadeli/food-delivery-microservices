using System.Text.RegularExpressions;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using LinqKit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver;
using Scrutor;

namespace BuildingBlocks.Web.Extensions;

public static class MinimalApiExtensions
{
    public static IServiceCollection AddMinimalEndpoints(
        this WebApplicationBuilder applicationBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        applicationBuilder.Services.Scan(scan => scan
            .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IMinimalEndpoint)))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .As<IMinimalEndpoint>()
            .WithLifetime(lifetime));

        return applicationBuilder.Services;
    }

    public static IServiceCollection AddMinimalEndpoints(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Scan(scan => scan
            .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IMinimalEndpoint)))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .As<IMinimalEndpoint>()
            .WithLifetime(lifetime));

        return services;
    }

    /// <summary>
    /// Map registered minimal apis.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapMinimalEndpoints(this IEndpointRouteBuilder builder)
    {
        var scope = builder.ServiceProvider.CreateScope();

        var endpoints = scope.ServiceProvider.GetServices<IMinimalEndpoint>().ToList();

        var versionGroups =
            endpoints.GroupBy(x => x.GroupName)
                .ToDictionary(x => x.Key, c => builder.MapApiGroup(c.Key).WithTags(c.Key));

        var versionSubGroups = endpoints.GroupBy(x => new {x.GroupName, x.PrefixRoute, x.Version})
            .ToDictionary(
                x => x.Key,
                c => versionGroups[c.Key.GroupName].MapGroup(c.Key.PrefixRoute).HasApiVersion(c.Key.Version));

        var endpointVersions = endpoints.GroupBy(x => new {x.GroupName, x.Version}).Select(x => new
        {
            Verion = x.Key.Version, x.Key.GroupName, Endpoints = x.Select(v => v)
        });

        foreach (var endpointVersion in endpointVersions)
        {
            var versionGroup = versionSubGroups
                .FirstOrDefault(x => x.Key.GroupName == endpointVersion.GroupName).Value;

            endpointVersion.Endpoints.ForEach(ep =>
            {
                ep.MapEndpoint(versionGroup);
            });
        }

        return builder;
    }
}
