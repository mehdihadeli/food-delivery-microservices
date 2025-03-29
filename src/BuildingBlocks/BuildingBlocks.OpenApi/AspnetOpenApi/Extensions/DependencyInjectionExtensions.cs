using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;

// https://github.com/dotnet/aspnet-api-versioning/issues/1115

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddAspnetOpenApi(this WebApplicationBuilder builder, string[] versions)
    {
        builder.Services.AddConfigurationOptions<OpenApiOptions>(nameof(OpenApiOptions));

        foreach (var documentName in versions)
        {
            builder.Services.AddOpenApi(
                documentName,
                options =>
                {
                    options.AddDocumentTransformer<OpenApiVersioningDocumentTransformer>();
                    options.AddDocumentTransformer<SecuritySchemeDocumentTransformer>();

                    options.AddOperationTransformer<OpenApiDefaultValuesOperationTransformer>();
                    options.AddSchemaTransformer<EnumSchemaTransformer>();
                }
            );
        }

        return builder;
    }
}
