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
    IEnumerable<IProblemDetailMapper>? problemDetailMappers
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "[{Handler}] Could not process a request on machine {MachineName}. TraceId: {TraceId}",
            nameof(DefaultExceptionHandler),
            Environment.MachineName,
            traceId
        );

        int statusCode = 0;

        if (problemDetailMappers is not null && problemDetailMappers.Any())
        {
            foreach (var problemDetailMapper in problemDetailMappers)
            {
                statusCode = problemDetailMapper.GetMappedStatusCodes(exception);
            }
        }
        else
        {
            statusCode = new DefaultProblemDetailMapper().GetMappedStatusCodes(exception);
        }

        httpContext.Response.StatusCode = statusCode == 0 ? httpContext.Response.StatusCode : statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            PopulateNewProblemDetail(statusCode, httpContext, exception, traceId),
            cancellationToken: cancellationToken
        );

        return true;
    }

    private ProblemDetails PopulateNewProblemDetail(
        int code,
        HttpContext httpContext,
        Exception exception,
        string traceId
    )
    {
        var extensions = new Dictionary<string, object?> { { "traceId", traceId } };

        // Add stackTrace in development mode for debugging purposes
        if (webHostEnvironment.IsDevelopment())
        {
            extensions["stackTrace"] = exception.StackTrace;
        }

        // type will fill automatically by .net core
        var problem = TypedResults
            .Problem(
                statusCode: code,
                detail: exception.Message,
                title: exception.GetType().Name,
                instance: $"{httpContext.Request.Method} {httpContext.Request.Path}",
                extensions: extensions
            )
            .ProblemDetails;

        return problem;
    }
}
