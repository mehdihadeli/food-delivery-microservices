using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.Shared.Helpers;

public static class TestHostedServiceExtensions
{
    public static void StartHostedServices(this IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
            {
                var hostedServices = serviceProvider.GetRequiredService<IEnumerable<IHostedService>>();
                foreach (var hostedService in hostedServices)
                {
                    await hostedService.StartAsync(cancellationToken);
                }
            }, cancellationToken)
            .GetAwaiter().GetResult();
    }
}