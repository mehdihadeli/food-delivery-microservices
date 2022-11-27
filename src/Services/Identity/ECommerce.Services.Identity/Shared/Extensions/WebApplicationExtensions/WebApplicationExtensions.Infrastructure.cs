using BuildingBlocks.HealthCheck;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Web.Extensions;
using Hellang.Middleware.ProblemDetails;
using Serilog;

namespace ECommerce.Services.Identity.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static async Task UseInfrastructure(this WebApplication app)
    {
        // orders for middlewares is important and problemDetails middleware should be placed on top
        app.UseProblemDetails();
        app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = LogEnricher.EnrichFromRequest);
        app.UseRequestLogContextMiddleware();

        app.UseAuthentication();
        app.UseAuthorization();

        await app.UsePostgresPersistenceMessage(app.Logger);
        app.UseCustomRateLimit();

        if (app.Environment.IsTest() == false)
        {
            app.UseCustomHealthCheck();
            app.UseIdentityServer();
        }

        // Configure the prometheus endpoint for scraping metrics
        // NOTE: This should only be exposed on an internal port!
        // .RequireHost("*:9100");
        app.MapPrometheusScrapingEndpoint();
    }
}
