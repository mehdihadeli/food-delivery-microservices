using System.Diagnostics;
using System.Reflection;
using BuildingBlocks.Abstractions.Web.Problem;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Scrutor;

namespace BuildingBlocks.Web.ProblemDetail;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/
public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomProblemDetails(
        this IHostApplicationBuilder builder,
        Assembly[] scanAssemblies,
        Action<ProblemDetailsOptions>? configure = null,
        bool useExceptionHandler = true,
        bool useCustomProblemDetailsService = false
    )
    {
        if (useExceptionHandler)
        {
            builder.Services.AddExceptionHandler<DefaultExceptionHandler>();
            builder.Services.AddProblemDetails();
        }
        else if (useCustomProblemDetailsService)
        {
            // Must be registered BEFORE AddProblemDetails because AddProblemDetails internally uses TryAddSingleton for adding default implementation for IProblemDetailsWriter
            builder.Services.AddSingleton<IProblemDetailsService, ProblemDetailsService>();
            builder.Services.AddSingleton<IProblemDetailsWriter, ProblemDetailsWriter>();

            builder.Services.AddProblemDetails(configure);
        }
        else
        {
            builder.Services.AddProblemDetails(c =>
            {
                c.CustomizeProblemDetails = context =>
                {
                    IExceptionHandlerFeature? exceptionFeature =
                        context.HttpContext.Features.Get<IExceptionHandlerFeature>();

                    Exception? exception = exceptionFeature?.Error ?? context.Exception;

                    var mappers = context.HttpContext.RequestServices.GetServices<IProblemDetailMapper>();

                    var webHostEnvironment =
                        context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

                    CreateProblemDetailFromException(context, webHostEnvironment, exception, mappers);
                };
            });
        }

        RegisterAllMappers(builder.Services, scanAssemblies);

        return builder;
    }

    private static void CreateProblemDetailFromException(
        ProblemDetailsContext context,
        IWebHostEnvironment webHostEnvironment,
        Exception? exception,
        IEnumerable<IProblemDetailMapper>? problemDetailMappers
    )
    {
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        int statusCode =
            problemDetailMappers?.Select(m => m.GetMappedStatusCodes(exception)).FirstOrDefault()
            ?? new DefaultProblemDetailMapper().GetMappedStatusCodes(exception);

        context.HttpContext.Response.StatusCode = statusCode;

        context.ProblemDetails = PopulateNewProblemDetail(
            statusCode,
            context.HttpContext,
            webHostEnvironment,
            exception,
            traceId
        );
    }

    private static ProblemDetails PopulateNewProblemDetail(
        int code,
        HttpContext httpContext,
        IWebHostEnvironment webHostEnvironment,
        Exception? exception,
        string traceId
    )
    {
        var extensions = new Dictionary<string, object?> { { "traceId", traceId } };

        // Add stackTrace in development mode for debugging purposes
        if (webHostEnvironment.IsDevelopment() && exception is { })
        {
            extensions["stackTrace"] = exception.StackTrace;
        }

        // type will fill automatically by .net core
        var problem = TypedResults
            .Problem(
                statusCode: code,
                detail: exception?.Message,
                title: exception?.GetType().Name,
                instance: $"{httpContext.Request.Method} {httpContext.Request.Path}",
                extensions: extensions
            )
            .ProblemDetails;

        return problem;
    }

    private static void RegisterAllMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo<IProblemDetailMapper>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );
    }
}
