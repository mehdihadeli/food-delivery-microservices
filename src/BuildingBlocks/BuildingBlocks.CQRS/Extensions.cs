using System.Reflection;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Core.CQRS.Command;
using BuildingBlocks.Core.CQRS.Query;
using MediatR;

namespace BuildingBlocks.CQRS;

public static class Extensions
{
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Assembly[]? assemblies = null,
        Action<IServiceCollection>? doMoreActions = null)
    {
        services.AddMediatR(
            assemblies ?? new[] { Assembly.GetCallingAssembly() },
            x =>
            {
                x.AsScoped();
            });

        services.AddScoped<ICommandProcessor, CommandProcessor>()
            .AddScoped<IQueryProcessor, QueryProcessor>();

        doMoreActions?.Invoke(services);

        return services;
    }
}
