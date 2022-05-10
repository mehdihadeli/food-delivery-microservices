using BuildingBlocks.Monitoring;

namespace Store.Services.Customers.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static Task<IApplicationBuilder> UseInfrastructure(this IApplicationBuilder app, ILogger logger)
    {
        app.UseMonitoring();

        return Task.FromResult(app);
    }
}
