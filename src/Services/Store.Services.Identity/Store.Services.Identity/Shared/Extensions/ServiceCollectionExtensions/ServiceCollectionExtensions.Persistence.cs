using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Store.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder, string connString)
    {
        AddStorage(builder.Services, connString);

        return builder;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, string connString)
    {
        return services;
    }
}
