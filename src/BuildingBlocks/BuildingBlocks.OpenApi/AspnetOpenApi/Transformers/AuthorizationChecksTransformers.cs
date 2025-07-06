using BuildingBlocks.Core.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Transformers;

public class AuthorizationChecksTransformers : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var oauthOptions = context.ApplicationServices.GetRequiredService<IOptions<OAuthOptions>>();

        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (!metadata.OfType<IAuthorizeData>().Any())
        {
            return Task.CompletedTask;
        }

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

        var oAuthScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
        };

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new() { [oAuthScheme] = oauthOptions.Value.Scopes },
        };

        return Task.CompletedTask;
    }
}
