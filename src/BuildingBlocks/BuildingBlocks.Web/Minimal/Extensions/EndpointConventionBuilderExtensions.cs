using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Minimal.Extensions;

public static class EndpointConventionBuilderExtensions
{
    public static RouteHandlerBuilder Produces(
        this RouteHandlerBuilder builder,
        string description,
        int statusCode,
        Type? responseType = null,
        string? contentType = null,
        params string[] additionalContentTypes
    )
    {
        // WithOpenApi should placed before versioning and other things - this fixed in Aps.Versioning.Http 7.0.0-preview.1
        builder.WithOpenApi(operation =>
        {
            operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)].Description = description;
            return operation;
        });

        builder.Produces(
            statusCode,
            responseType,
            contentType: contentType,
            additionalContentTypes: additionalContentTypes
        );

        // // with suing https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcoreannotations
        // builder.WithMetadata(new SwaggerResponseAttribute(statusCode, description, responseType));
        return builder;
    }

    public static RouteHandlerBuilder Produces<TResponse>(
        this RouteHandlerBuilder builder,
        string description,
        int statusCode,
        string? contentType = null,
        params string[] additionalContentTypes
    )
    {
        builder.WithOpenApi(operation =>
        {
            operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)].Description = description;
            return operation;
        });

        builder.Produces<TResponse>(
            statusCode,
            contentType: contentType,
            additionalContentTypes: additionalContentTypes
        );

        // // with suing https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcoreannotations
        // builder.WithMetadata(new SwaggerResponseAttribute(
        //     statusCode,
        //     description,
        //     typeof(TResponse)));
        return builder;
    }

    public static RouteHandlerBuilder ProducesProblem(
        this RouteHandlerBuilder builder,
        string description,
        int statusCode,
        string? contentType = null
    )
    {
        builder.WithOpenApi(operation =>
        {
            operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)].Description = description;
            return operation;
        });

        builder.ProducesProblem(statusCode, contentType: contentType);

        // // with suing https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcoreannotations
        // builder.WithMetadata(
        //     new SwaggerResponseAttribute(
        //         statusCode,
        //         description,
        //         typeof(ProblemDetails)));
        return builder;
    }

    public static RouteHandlerBuilder ProducesValidationProblem(
        this RouteHandlerBuilder builder,
        string description,
        int statusCode = StatusCodes.Status400BadRequest,
        string? contentType = null
    )
    {
        builder.WithOpenApi(operation =>
        {
            operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)].Description = description;
            return operation;
        });
        builder.ProducesValidationProblem(statusCode, contentType: contentType);

        return builder;
    }

    public static RouteHandlerBuilder WithSummaryAndDescription(
        this RouteHandlerBuilder builder,
        string summary,
        string description
    )
    {
        builder.WithOpenApi(operation =>
        {
            operation.Summary = summary;
            operation.Description = description;
            return operation;
        });

        //// with suing https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckleaspnetcoreannotations
        // builder.WithMetadata(new SwaggerOperationAttribute("Summary...", "Description..."))
        return builder;
    }
}
