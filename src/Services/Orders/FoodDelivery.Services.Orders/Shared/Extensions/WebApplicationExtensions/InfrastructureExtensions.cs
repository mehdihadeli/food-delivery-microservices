using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.Core.Web.HeaderPropagation.Extensions;
using BuildingBlocks.HealthCheck;
using BuildingBlocks.Observability.Extensions;
using BuildingBlocks.Web.Extensions;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.Extensions.Primitives;

namespace FoodDelivery.Services.Orders.Shared.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static void UseInfrastructure(this WebApplication app)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
        // https://github.com/dotnet/aspnetcore/pull/26567
        app.UseExceptionHandler(new ExceptionHandlerOptions { AllowStatusCode404Response = true });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || app.Environment.IsTest())
        {
            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errrors
            app.UseDeveloperExceptionPage();
        }

        app.UseCustomCors();

        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security
        app.UseAuthentication();
        app.UseAuthorization();

        // https://aurelien-riv.github.io/aspnetcore/2022/11/09/aspnet-grafana-loki-telemetry-microservice-correlation.html
        // https://www.nuget.org/packages/Microsoft.AspNetCore.HeaderPropagation
        // https://gist.github.com/davidfowl/c34633f1ddc519f030a1c0c5abe8e867
        // https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/HeaderPropagation/test/HeaderPropagationIntegrationTest.cs
        app.UseHeaderPropagation();

        app.MapCustomHealthChecks();

        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks#non-development-environments
        app.UseRequestTimeouts();
        app.UseOutputCache();

        app.UseObservability();
    }
}
