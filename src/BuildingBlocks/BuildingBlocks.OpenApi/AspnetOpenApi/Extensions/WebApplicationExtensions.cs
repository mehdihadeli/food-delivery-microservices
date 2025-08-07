using BuildingBlocks.Core.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseAspnetOpenApi(this WebApplication app)
    {
        app.MapOpenApi();

        if (OpenApiOptions.IsOpenApiBuild || app.Environment.IsBuild())
            Environment.Exit(0);

        if (!app.Environment.IsDevelopment())
            return app;

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
        // https://github.com/scalar/scalar/blob/fbef7e1ee82d7c9e84bc42407e309642dcec5552/documentation/integrations/aspnetcore.md
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
