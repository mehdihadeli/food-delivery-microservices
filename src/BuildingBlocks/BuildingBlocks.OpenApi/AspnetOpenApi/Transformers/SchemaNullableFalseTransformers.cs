using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Transformers;

// This extension method adds a schema transformer that sets "nullable" to false for all optional properties.
public class SchemaNullableFalseTransformers : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (schema.Properties is not null)
        {
            foreach (var property in schema.Properties)
            {
                if (schema.Required?.Contains(property.Key) != true)
                {
                    property.Value.Nullable = false;
                }
            }
        }

        return Task.CompletedTask;
    }
}
