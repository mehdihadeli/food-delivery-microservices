using System.Diagnostics;
using BuildingBlocks.Abstractions.Core.Diagnostics;
using BuildingBlocks.Core.Diagnostics.CoreDiagnostics.Commands;
using BuildingBlocks.Core.Diagnostics.CoreDiagnostics.Query;
using BuildingBlocks.Core.Extensions;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Core.Diagnostics.Extensions;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddDiagnostics(this WebApplicationBuilder builder)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        var coreOptions = builder.Configuration.BindOptions<CoreOptions>();
        ArgumentException.ThrowIfNullOrEmpty(coreOptions.InstrumentationName);
        DiagnosticsConstant.ApplicationInstrumentationName = coreOptions.InstrumentationName;

        builder.Services.AddSingleton<IDiagnosticsProvider, DiagnosticsProvider>();

        builder.Services.AddTransient<CommandHandlerActivity>();
        builder.Services.AddTransient<CommandHandlerMetrics>();
        builder.Services.AddTransient<QueryHandlerActivity>();
        builder.Services.AddTransient<QueryHandlerMetrics>();

        return builder;
    }
}
