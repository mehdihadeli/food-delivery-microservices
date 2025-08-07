using System.Diagnostics;
using BuildingBlocks.Abstractions.Web.Problem;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Web.ProblemDetail;

// ref: https://anthonygiretti.com/2023/06/14/asp-net-core-8-improved-exception-handling-with-iexceptionhandler/
public class DefaultExceptionHandler(
    ILogger<DefaultExceptionHandler> logger,
    IWebHostEnvironment webHostEnvironment,
    IEnumerable<IProblemDetailMapper> problemDetailMappers,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "An unexpected error occurred");

        var problemDetail = CreateProblemDetailFromException(httpContext, exception);

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetail,
        };

        await problemDetailsService.WriteAsync(context);

        return true;
    }

    private ProblemDetails CreateProblemDetailFromException(HttpContext context, Exception? exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (exception is { })
        {
            logger.LogError(
                exception,
                "Could not process a request on machine {MachineName}. TraceId: {TraceId}",
                Environment.MachineName,
                traceId
            );
        }

        int statusCode = !problemDetailMappers.Any()
            ? new DefaultProblemDetailMapper().GetMappedStatusCodes(exception)
            : problemDetailMappers.Select(m => m.GetMappedStatusCodes(exception)).FirstOrDefault();

        context.Response.StatusCode = statusCode;

        return PopulateNewProblemDetail(statusCode, context, exception, traceId);
    }

    private ProblemDetails PopulateNewProblemDetail(
        int code,
        HttpContext httpContext,
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
}
