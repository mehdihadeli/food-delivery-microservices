using BuildingBlocks.Core.Diagnostics.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages;
using BuildingBlocks.Core.Security;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.OpenTelemetry.Extensions;
using BuildingBlocks.Resiliency;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.ProblemDetail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoodDelivery.Services.Shared.Extensions;

public static class HostApplicationBuilderExtensions
{
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddBasicServiceDefaults();

        builder.Services.AddHttpContextAccessor();

        builder.AddCustomProblemDetails();

        builder.Services.AddServiceDiscovery();

        builder.AddCustomResiliency(false);

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

    private static IHostApplicationBuilder AddBasicServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();

        builder.AddDiagnostics(builder.Configuration.GetValue<string>("InstrumentationName") ?? "food-delivery");

        builder.AddDefaultOpenTelemetry();

        return builder;
    }
}
