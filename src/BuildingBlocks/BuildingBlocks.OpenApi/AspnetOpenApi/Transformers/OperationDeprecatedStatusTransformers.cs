using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Transformers;

public class OperationDeprecatedStatusTransformers : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var apiDescription = context.Description;
        operation.Deprecated |= apiDescription.IsDeprecated();

        return Task.CompletedTask;
    }
}
