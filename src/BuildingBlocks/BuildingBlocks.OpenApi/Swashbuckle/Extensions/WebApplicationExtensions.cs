using BuildingBlocks.Core.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace BuildingBlocks.OpenApi.Swashbuckle.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSwaggerOpenApi(this WebApplication app)
    {
        // show in both production and development
        app.MapGet("/", () => $"{app.Environment.ApplicationName} is started.").ExcludeFromDescription();

        if (!app.Environment.IsDevelopment())
            return app;

        // we should not see openapi docs in none development mode
        app.UseSwagger();

        var descriptions = app.DescribeApiVersions();

        // Add swagger ui
        app.UseSwaggerUI(options =>
        {
            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var openApiUrl = $"/openapi/{description.GroupName}.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(openApiUrl, name);
            }
        });

        // Add scalar ui
        app.MapScalarApiReference(scalarOptions =>
        {
            scalarOptions.WithOpenApiRoutePattern("/openapi/{documentName}.json");
            scalarOptions.Theme = ScalarTheme.BluePlanet;
            // Disable default fonts to avoid download unnecessary fonts
            scalarOptions.DefaultFonts = false;
        });

        return app;
    }
}
