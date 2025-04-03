using System.Reflection;
using BuildingBlocks.Abstractions.Core;
using BuildingBlocks.Core.Commands;
using BuildingBlocks.Core.Diagnostics.Extensions;
using BuildingBlocks.Core.Events.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Messages.Extensions;
using BuildingBlocks.Core.Paging;
using BuildingBlocks.Core.Persistence;
using BuildingBlocks.Core.Queries;
using BuildingBlocks.Core.Resiliency;
using BuildingBlocks.Core.Serialization;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sieve.Services;

namespace BuildingBlocks.Core.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddCore(this WebApplicationBuilder builder)
    {
        // Find assemblies that reference the current assembly
        var referencingAssemblies = Assembly.GetCallingAssembly().GetReferencingAssemblies();

        builder.Services.AddValidatedOptions<CoreOptions>();

        builder.AddDiagnostics();

        builder.Services.TryAddScoped<ISieveProcessor, ApplicationSieveProcessor>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddDefaultSerializer();

        builder.Services.AddCommandBus();

        builder.Services.AddQueryBus();

        builder.Services.AddEvents(referencingAssemblies);

        builder.Services.AddMessages(referencingAssemblies);

        builder.Services.AddPersistenceCore(referencingAssemblies);

        builder.AddCoreResiliency();

        // will override by services for more customization
        builder.Services.AddHeaderPropagation();

        builder.Services.TryAddScoped<IMediator, NullMediator>();

        builder.Services.TryAddSingleton<IExclusiveLock, ExclusiveLock>();

        return builder;
    }
}
