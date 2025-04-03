using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.OpenApi.Swashbuckle;

// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269#issuecomment-577182931
// `SchemaFilter` is used to customize `schemas` in the OpenAPI document.
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = context.Type;

        // Check if the type is an enum
        if (!enumType.IsEnum)
            return;

        // Clear the default enum values
        schema.Enum.Clear();

        // Add only string representations of the enum values
        Enum.GetNames(enumType).ToList().ForEach(name => schema.Enum.Add(new OpenApiString(name)));

        // Set the schema type explicitly to "string"
        schema.Type = "string";
    }
}
