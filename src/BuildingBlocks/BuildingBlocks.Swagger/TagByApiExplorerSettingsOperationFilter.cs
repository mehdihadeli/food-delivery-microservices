using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Swagger;

public class TagByApiExplorerSettingsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var apiExplorerSettings = controllerActionDescriptor
                .ControllerTypeInfo.GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), true)
                .Cast<ApiExplorerSettingsAttribute>().FirstOrDefault();
            if (apiExplorerSettings != null && !string.IsNullOrWhiteSpace(apiExplorerSettings.GroupName))
            {
                operation.Tags = new List<OpenApiTag> {new() {Name = apiExplorerSettings.GroupName}};
            }
            if (controllerActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is ApiExplorerSettingsAttribute) is
                ApiExplorerSettingsAttribute apiExplorerSettingsEndpoint)
            {
                operation.Tags = new List<OpenApiTag> {new() {Name = apiExplorerSettingsEndpoint.GroupName}};
            }
        }
    }
}