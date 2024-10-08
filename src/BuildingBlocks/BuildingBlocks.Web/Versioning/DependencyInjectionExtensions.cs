using Asp.Versioning;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Versioning;

public static class DependencyInjectionExtensions
{
    public static WebApplicationBuilder AddCustomVersioning(
        this WebApplicationBuilder builder,
        Action<ApiVersioningOptions>? configurator = null
    )
    {
        // https://www.meziantou.net/versioning-an-asp-net-core-api.htm
        // https://dotnetthoughts.net/aspnetcore-api-versioning-with-net-6-minimal-apis/
        // https://im5tu.io/article/2022/10/asp.net-core-versioning-minimal-apis/
        // https://www.youtube.com/watch?v=YRJGKyzjFlY
        // https://www.nuget.org/packages/Asp.Versioning.Http

        // Support versioning in minimal apis with (Asp.Versioning.Http) dll
        builder
            .Services.AddApiVersioning(options =>
            {
                // Add the headers "api-supported-versions" and "api-deprecated-versions"
                // This is better for discoverability
                options.ReportApiVersions = true;

                // AssumeDefaultVersionWhenUnspecified should only be enabled when supporting legacy services that did not previously
                // support API versioning. Forcing existing clients to specify an explicit API version for an
                // existing service introduces a breaking change. Conceptually, clients in this situation are
                // bound to some API version of a service, but they don't know what it is and never explicit request it.
                options.AssumeDefaultVersionWhenUnspecified = true;

                // the default value of `DefaultApiVersion` is  `ApiVersion(1, new int?(0)`
                options.DefaultApiVersion = new ApiVersion(1, 0);

                // default `ApiVersionReader` is combine of both `QueryStringApiVersionReader` and `UrlSegmentApiVersionReader`
                // Defines how an API version is read from the current HTTP request
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("api-version"),
                    new QueryStringApiVersionReader(),
                    new UrlSegmentApiVersionReader()
                );

                configurator?.Invoke(options);
            })
            .AddApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note1: this option is only necessary when versioning by url segment.
                // note2:if we set it to true version will infer automatically in swagger based on mapped versions to api (swagger infer api versions in different tab in header Swagger UI),
                // but if we set it to false we should put version for api endpoint manually.
                // https://github.com/dotnet/aspnet-api-versioning/issues/909
                options.SubstituteApiVersionInUrl = true;
            })
            // Support versioning in mvc with with (Asp.Versioning.Mvc.ApiExplorer) dll
            .AddMvc(); // https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer

        return builder;
    }
}
