using System.Reflection;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Events.Extensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Messages.Extensions;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Persistence;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Core.Serialization;
using BuildingBlocks.Core.Serialization.NewtonsoftSerializer;
using Mediator;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sieve.Services;

namespace BuildingBlocks.Core.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCoreServices(this IHostApplicationBuilder builder)
    {
        // Find assemblies that reference the current assembly
        var referencingAssemblies = Assembly.GetCallingAssembly().GetReferencingAssemblies();

        builder.Services.TryAddScoped<ISieveProcessor, ApplicationSieveProcessor>();

        builder.Services.AddDefaultSerializer();

        builder.Services.AddCommandBus();

        builder.Services.AddQueryBus();

        builder.Services.AddEvents(referencingAssemblies);

        builder.Services.AddMessages(referencingAssemblies);

        builder.Services.ScanAndRegisterDbExecutors(referencingAssemblies);

        builder.Services.TryAddScoped<IMediator, NullMediator>();

        return builder;
    }
}
