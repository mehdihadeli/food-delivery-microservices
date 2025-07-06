using System.Text;
using Asp.Versioning.ApiExplorer;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Transformers;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0#customize-openapi-documents-with-transformers
public class OpenApiVersioningDocumentTransformer(
    IApiVersionDescriptionProvider apiVersionDescriptionProvider,
    IOptions<OpenApiOptions> options
) : IOpenApiDocumentTransformer
{
    private readonly OpenApiOptions? _openApiOptions = options.Value;

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var apiDescription = apiVersionDescriptionProvider.ApiVersionDescriptions.SingleOrDefault(description =>
            description.GroupName == context.DocumentName
        );

        if (apiDescription is null)
        {
            return Task.CompletedTask;
        }

        document.Info.License = new OpenApiLicense
        {
            Name = _openApiOptions?.LicenseName,
            Url = _openApiOptions?.LicenseUrl,
        };

        document.Info.Contact = new OpenApiContact
        {
            Name = _openApiOptions?.AuthorName,
            Url = _openApiOptions?.AuthorUrl,
            Email = _openApiOptions?.AuthorEmail,
        };

        document.Info.Version = apiDescription.ApiVersion.ToString();

        document.Info.Title = _openApiOptions?.Title;

        document.Info.Description = apiDescription.BuildDescription(_openApiOptions?.Description);

        return Task.CompletedTask;
    }
}
