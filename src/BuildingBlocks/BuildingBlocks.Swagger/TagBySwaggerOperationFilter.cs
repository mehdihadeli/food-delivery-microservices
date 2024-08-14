using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Swagger;

public class TagBySwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
            return;

        var apiExplorerSettings = controllerActionDescriptor
            .ControllerTypeInfo.GetCustomAttributes(typeof(SwaggerOperationAttribute), true)
            .Cast<SwaggerOperationAttribute>()
            .FirstOrDefault();
        if (apiExplorerSettings != null && apiExplorerSettings.Tags.Length != 0)
        {
            operation.Tags = apiExplorerSettings.Tags.Select(x => new OpenApiTag { Name = x }).ToList();
        }

        if (
            controllerActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is SwaggerOperationAttribute)
                is SwaggerOperationAttribute swaggerOperationEndpoint
            && swaggerOperationEndpoint.Tags.Length != 0
        )
        {
            operation.Tags = swaggerOperationEndpoint.Tags.Select(x => new OpenApiTag { Name = x }).ToList();
        }
    }
}
