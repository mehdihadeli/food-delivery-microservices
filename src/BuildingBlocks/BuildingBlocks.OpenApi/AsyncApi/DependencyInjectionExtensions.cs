using Asp.Versioning.ApiExplorer;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Saunter;

namespace BuildingBlocks.OpenApi.AsyncApi;

public static class DependencyInjectionExtensions
{
    public static void AddAsyncApi(this IHostApplicationBuilder builder, IList<Type> types)
    {
        var services = builder.Services;

        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.AssemblyMarkerTypes = types;

            using var sp = services.BuildServiceProvider();
            var document = sp.GetRequiredService<OpenApiOptions>();

            foreach (var version in sp.GetApiVersionDescription())
            {
                options.AsyncApi = new()
                {
                    Info = new(document.Title, version.ApiVersion.ToString())
                    {
                        Description = version.BuildDescription(document.Description),
                        License = new(document.LicenseName) { Url = document.LicenseUrl.ToString() },
                        Contact = new()
                        {
                            Name = document.AuthorName,
                            Url = document.AuthorUrl?.ToString(),
                            Email = document.AuthorEmail,
                        },
                    },
                };
            }
        });
    }

    private static IReadOnlyList<ApiVersionDescription> GetApiVersionDescription(this IServiceProvider provider)
    {
        return provider.GetService<IApiVersionDescriptionProvider>()?.ApiVersionDescriptions ?? [new(new(1, 0), "v1")];
    }

    public static void UseAsyncApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.MapAsyncApiDocuments();
        app.MapAsyncApiUi();
    }
}
