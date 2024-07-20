using BuildingBlocks.HealthCheck;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Middlewares.CaptureExceptionMiddleware;
using BuildingBlocks.Web.Middlewares.RequestLogContextMiddleware;
using Serilog;

namespace FoodDelivery.Services.Catalogs.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static async Task UseInfrastructure(this WebApplication app)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
        // Does nothing if a response body has already been provided. when our next `DeveloperExceptionMiddleware` is written response for exception (in dev mode) when we back to `ExceptionHandlerMiddlewareImpl` because `context.Response.HasStarted` it doesn't do anything
        // By default `ExceptionHandlerMiddlewareImpl` middleware register original exceptions with `IExceptionHandlerFeature` feature, we don't have this in `DeveloperExceptionPageMiddleware` and we should handle it with a middleware like `CaptureExceptionMiddleware`
        // Just for handling exceptions in production mode
        // https://github.com/dotnet/aspnetcore/pull/26567
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsTest())
        {
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errrors
            app.UseDeveloperExceptionPage();

            // https://github.com/dotnet/aspnetcore/issues/4765
            // https://github.com/dotnet/aspnetcore/pull/47760
            // .net 8 will add `IExceptionHandlerFeature`in `DisplayExceptionContent` and `SetExceptionHandlerFeatures` methods `DeveloperExceptionPageMiddlewareImpl` class, exact functionality of CaptureException
            // bet before .net 8 preview 5 we should add `IExceptionHandlerFeature` manually with our `UseCaptureException`
            app.UseCaptureException();
        }

        // this middleware should be first middleware
        // request logging just log in information level and above as default
        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest;

            // this level wil use for request logging
            // https://andrewlock.net/using-serilog-aspnetcore-in-asp-net-core-3-excluding-health-check-endpoints-from-serilog-request-logging/#customising-the-log-level-used-for-serilog-request-logs
            opts.GetLevel = LogEnricher.GetLogLevel;
        });

        app.UseRequestLogContextMiddleware();

        app.UseCustomCors();

        app.UseAuthentication();
        app.UseAuthorization();

        await app.UsePostgresPersistenceMessage(app.Logger);

        await app.MigrateDatabases();

        app.UseCustomRateLimit();

        if (app.Environment.IsTest() == false)
            app.UseCustomHealthCheck();

        // Configure the prometheus endpoint for scraping metrics
        // NOTE: This should only be exposed on an internal port!
        // .RequireHost("*:9100");
        app.MapPrometheusScrapingEndpoint();
    }
}
