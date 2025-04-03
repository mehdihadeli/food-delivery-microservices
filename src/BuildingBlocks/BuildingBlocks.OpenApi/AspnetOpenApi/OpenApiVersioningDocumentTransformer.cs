using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi.AspnetOpenApi;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0#customize-openapi-documents-with-transformers
public class OpenApiVersioningDocumentTransformer(
    IApiVersionDescriptionProvider apiVersionDescriptionProvider,
    IOptions<OpenApiOptions> options
) : IOpenApiDocumentTransformer
{
    private readonly OpenApiOptions _openApiOptions = options.Value;

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

        // note: you might choose to skip or document deprecated API versions differently
        var openapiInfo = CreateInfoForApiVersion(apiDescription);

        document.Info = openapiInfo;

        return Task.CompletedTask;
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var text = new StringBuilder(_openApiOptions.Description);
        var info = new OpenApiInfo
        {
            Title = _openApiOptions.Title,
            Description = _openApiOptions.Description,
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact
            {
                Name = _openApiOptions.ContactUserName,
                Email = _openApiOptions.ContactEmail,
            },
            License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") },
        };

        if (description.IsDeprecated)
        {
            text.Append(" This API version has been deprecated.");
        }

        if (description.SunsetPolicy is { } policy)
        {
            if (policy.Date is { } when)
            {
                text.Append(" The API will be sunset on ").Append(when.Date.ToShortDateString()).Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                var rendered = false;

                foreach (var link in policy.Links)
                {
                    if (link.Type == "text/html")
                    {
                        if (!rendered)
                        {
                            text.Append("<h4>Links</h4><ul>");
                            rendered = true;
                        }

                        text.Append("<li><a href=\"");
                        text.Append(link.LinkTarget.OriginalString);
                        text.Append("\">");
                        text.Append(
                            StringSegment.IsNullOrEmpty(link.Title)
                                ? link.LinkTarget.OriginalString
                                : link.Title.ToString()
                        );
                        text.Append("</a></li>");
                    }
                }

                if (rendered)
                {
                    text.Append("</ul>");
                }
            }
        }

        text.Append("<h4>Additional Information</h4>");
        info.Description = text.ToString();

        return info;
    }
}
