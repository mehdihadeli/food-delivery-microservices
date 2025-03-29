using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi.AspnetOpenApi;

public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var enumType = context.JsonTypeInfo.Type;

        // Check if the type is an enum
        if (!enumType.IsEnum)
            return Task.CompletedTask;

        // Clear the default enum values
        schema.Enum.Clear();

        // Add only string representations of the enum values
        Enum.GetNames(enumType).ToList().ForEach(name => schema.Enum.Add(new OpenApiString(name)));

        // Set the schema type explicitly to "string"
        schema.Type = "string";

        return Task.CompletedTask;
    }
}
