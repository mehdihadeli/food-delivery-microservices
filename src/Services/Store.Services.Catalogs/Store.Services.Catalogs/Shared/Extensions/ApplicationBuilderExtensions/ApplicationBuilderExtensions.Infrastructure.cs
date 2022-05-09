using BuildingBlocks.Monitoring;

namespace Store.Services.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> UseInfrastructure(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        app.UseMonitoring();

        return app;
    }
}
