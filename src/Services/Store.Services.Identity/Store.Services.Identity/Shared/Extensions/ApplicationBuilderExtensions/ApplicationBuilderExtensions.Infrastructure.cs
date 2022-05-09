using BuildingBlocks.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Store.Services.Identity.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> UseInfrastructure(this IApplicationBuilder app, ILogger logger)
    {
        app.UseMonitoring();

        return app;
    }
}
