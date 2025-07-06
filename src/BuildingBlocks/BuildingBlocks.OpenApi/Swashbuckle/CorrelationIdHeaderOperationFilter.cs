using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.OpenApi.Swashbuckle;

public class CorrelationIdHeaderOperationFilter : IOperationFilter
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // Add the Correlation ID header to the operation parameters
        operation.Parameters.Add(
            new OpenApiParameter
            {
                Name = CorrelationIdHeaderName,
                In = ParameterLocation.Header,
                Description = "Correlation ID for tracking requests across systems",
                Required = false, // Set to true if the header is mandatory
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("123e4567-e89b-12d3-a456-426614174000"),
                },
            }
        );
    }
}
