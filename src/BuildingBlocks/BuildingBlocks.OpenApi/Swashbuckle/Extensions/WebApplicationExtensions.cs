using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

namespace BuildingBlocks.OpenApi.Swashbuckle.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSwaggerOpenApi(this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            var descriptions = app.DescribeApiVersions();

            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var openApiUrl = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(openApiUrl, name);
            }
        });

        // Add scalar ui
        app.MapScalarApiReference(redocOptions =>
        {
            redocOptions.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
        });

        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var openApiUrl = $"/swagger/{description.GroupName}/swagger.json";

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
