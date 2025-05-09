using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
    // https://steven-giesel.com/blogPost/ce948083-974a-4c16-877f-246b8909fa6d
    // https://www.stevejgordon.co.uk/aspnet-core-dependency-injection-what-is-the-iserviceprovider-and-how-is-it-built
    // https://www.youtube.com/watch?v=8JkHgymp2R4

    /// <summary>
    /// Validate container dependencies.
    /// </summary>
    /// <param name="rootServiceProvider"></param>
    /// <param name="services"></param>
    /// <param name="assembliesToScan"></param>
    public static void ValidateDependencies(
        this IServiceProvider rootServiceProvider,
        IServiceCollection services,
        params Assembly[] assembliesToScan
    )
    {
        var scanAssemblies = assembliesToScan.Length != 0 ? assembliesToScan : [Assembly.GetExecutingAssembly()];

        // for resolving scoped based dependencies without errors
        using var scope = rootServiceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        foreach (var serviceDescriptor in services)
        {
            // Skip services that are not typically resolved directly or are special cases
            if (
                serviceDescriptor.ServiceType == typeof(IHostedService)
                || serviceDescriptor.ServiceType == typeof(IHostApplicationLifetime)
            )
            {
                continue;
            }

            try
            {
                var serviceType = serviceDescriptor.ServiceType;
                if (scanAssemblies.Contains(serviceType.Assembly))
                {
                    // Attempt to resolve the service
                    var service = sp.GetService(serviceType);

                    // Assert: Check that the service was resolved if it's not meant to be optional
                    if (
                        serviceDescriptor.ImplementationInstance == null
                        && serviceDescriptor.ImplementationFactory == null
                    )
                    {
                        service.NotBeNull();
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new($"Failed to resolve service {serviceDescriptor.ServiceType.FullName}: {ex.Message}", ex);
            }
        }
    }
}
