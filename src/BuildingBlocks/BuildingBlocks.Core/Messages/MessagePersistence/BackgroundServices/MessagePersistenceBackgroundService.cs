namespace BuildingBlocks.Core.Messages.MessagePersistence.BackgroundServices;

using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Web.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
public class MessagePersistenceBackgroundService(
    ILogger<MessagePersistenceBackgroundService> logger,
    IOptions<MessagePersistenceOptions> options,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime
) : BackgroundService
{
    private readonly MessagePersistenceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await lifetime.WaitForAppStartup(stoppingToken))
        {
            return;
        }

        logger.LogInformation("MessagePersistence Background Service is starting");

        await ProcessAsync(stoppingToken).ConfigureAwait(false);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("MessagePersistence Background Service is stopping");

        return base.StopAsync(cancellationToken);
    }

    private async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                // resolve `IMessagePersistenceService` service with ServiceScoped which registered as scoped
                var service = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();
                await service.ProcessAllAsync(stoppingToken).ConfigureAwait(false);
            }

            var delay = _options.Interval is { }
                ? TimeSpan.FromSeconds((int)_options.Interval)
                : TimeSpan.FromSeconds(30);

            await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
        }
    }
}
