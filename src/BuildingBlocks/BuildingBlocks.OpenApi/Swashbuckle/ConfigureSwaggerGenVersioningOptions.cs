using Asp.Versioning.ApiExplorer;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.OpenApi.Swashbuckle;

// https://andrewlock.net/simplifying-dependency-injection-for-iconfigureoptions-with-the-configureoptions-helper/
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-9.0

/// <summary>
/// Configures the Swagger generation options.
/// </summary>
/// <remarks>This allows API versioning to define a Swagger document per API version after the
/// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
public class ConfigureSwaggerGenVersioningOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
    private readonly OpenApiOptions? _openApiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureSwaggerGenVersioningOptions"/> class.
    /// </summary>
    /// <param name="apiVersionDescriptionProvider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
    /// <param name="options"></param>
    public ConfigureSwaggerGenVersioningOptions(
        IApiVersionDescriptionProvider apiVersionDescriptionProvider,
        IOptions<OpenApiOptions> options
    )
    {
        _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
        _openApiOptions = options.Value;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // add a swagger document for each discovered API version
        // note: you might choose to skip or document deprecated API versions differently
        foreach (var description in _apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            License = new OpenApiLicense { Name = _openApiOptions?.LicenseName, Url = _openApiOptions?.LicenseUrl },
            Contact = new OpenApiContact
            {
                Name = _openApiOptions?.AuthorName,
                Url = _openApiOptions?.AuthorUrl,
                Email = _openApiOptions?.AuthorEmail,
            },
            Version = description.ApiVersion.ToString(),
            Title = _openApiOptions?.Title,
            Description = description.BuildDescription(_openApiOptions?.Description),
        };

        return info;
    }
}
