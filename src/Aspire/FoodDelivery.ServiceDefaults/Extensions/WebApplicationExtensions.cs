using BuildingBlocks.HealthCheck;
using BuildingBlocks.OpenTelemetry.Extensions;
using Microsoft.AspNetCore.Builder;

namespace FoodDelivery.ServiceDefaults.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
        // https://github.com/dotnet/aspnetcore/pull/26567
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging/
        app.UseHttpLogging();

        // Handles non-exceptional status codes (e.g., 404 from Results.NotFound(), 401 from unauthorized access) and returns standardized ProblemDetails responses
        app.UseStatusCodePages();

        // https://aurelien-riv.github.io/aspnetcore/2022/11/09/aspnet-grafana-loki-telemetry-microservice-correlation.html
        // https://www.nuget.org/packages/Microsoft.AspNetCore.HeaderPropagation
        // https://gist.github.com/davidfowl/c34633f1ddc519f030a1c0c5abe8e867
        // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/HeaderPropagation/test/HeaderPropagationIntegrationTest.cs
        app.UseHeaderPropagation();

        app.MapDefaultOpenTelemetry();

        app.MapDefaultHealthChecks();

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        app.UseRequestTimeouts();
        app.UseOutputCache();

        return app;
    }
}
