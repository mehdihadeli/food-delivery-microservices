using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAspnetOpenApi(this WebApplication app)
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();

            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var openApiUrl = $"/openapi/{description.GroupName}.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(openApiUrl, name);
            }
        });

        // Add scalar ui
        app.MapScalarApiReference(redocOptions =>
        {
            redocOptions.WithOpenApiRoutePattern("/openapi/{documentName}.json");
        });

        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var openApiUrl = $"/openapi/{description.GroupName}.json";

            // Add Redoc ui
            app.UseReDoc(redocOptions =>
            {
                redocOptions.RoutePrefix = $"redoc/{description.GroupName}";
                redocOptions.SpecUrl(openApiUrl);
            });
        }

        return app;
    }
}
