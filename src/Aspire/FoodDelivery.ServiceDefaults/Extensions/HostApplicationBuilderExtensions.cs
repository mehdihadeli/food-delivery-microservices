using System.Reflection;
using CloudNativeKit.Core.Diagnostics.Extensions;
using CloudNativeKit.Core.Extensions;
using CloudNativeKit.Core.Extensions.ServiceCollectionExtensions;
using CloudNativeKit.Core.Messages;
using CloudNativeKit.Core.Security;
using CloudNativeKit.HealthCheck;
using CloudNativeKit.OpenTelemetry.Extensions;
using CloudNativeKit.Resiliency;
using CloudNativeKit.Web.Extensions;
using CloudNativeKit.Web.ProblemDetail;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoodDelivery.ServiceDefaults.Extensions;

public static class HostApplicationBuilderExtensions
{
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();

        builder.AddDiagnostics(builder.Configuration.GetValue<string>("InstrumentationName") ?? "food-delivery");

        builder.AddDefaultOpenTelemetry();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/
        builder.Services.AddHttpLogging(o =>
        {
            o.CombineLogs = true;
            o.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
        });

        builder.Services.AddHttpContextAccessor();

        builder.AddCustomProblemDetails(scanAssemblies: Assembly.GetCallingAssembly().GetReferencingAssemblies());

        builder.AddCustomResiliency();

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        builder.Services.AddRequestTimeouts();
        builder.Services.AddOutputCache();

        builder.AddCompression();

        builder.Services.AddValidationOptions<OAuthOptions>();

        // https://aurelien-riv.github.io/aspnetcore/2022/11/09/aspnet-grafana-loki-telemetry-microservice-correlation.html
        // https://www.nuget.org/packages/Microsoft.AspNetCore.HeaderPropagation
        // https://gist.github.com/davidfowl/c34633f1ddc519f030a1c0c5abe8e867
        // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/HeaderPropagation/test/HeaderPropagationIntegrationTest.cs
        builder.Services.AddHeaderPropagation(options =>
        {
            options.Headers.Add(MessageHeaders.CorrelationId);
            options.Headers.Add(MessageHeaders.CausationId);
        });

        return builder;
    }
}
