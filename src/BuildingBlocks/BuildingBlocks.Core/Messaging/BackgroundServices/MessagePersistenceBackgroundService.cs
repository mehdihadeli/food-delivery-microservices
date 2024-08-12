using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Web.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Messaging.BackgroundServices;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
public class MessagePersistenceBackgroundService(
    ILogger<MessagePersistenceBackgroundService> logger,
    IOptions<MessagePersistenceOptions> options,
    IServiceProvider serviceProvider,
    IHostApplicationLifetime lifetime,
    IMachineInstanceInfo machineInstanceInfo
) : BackgroundService
{
    private readonly MessagePersistenceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await lifetime.WaitForAppStartup(stoppingToken))
        {
            return;
        }

        logger.LogInformation(
            "MessagePersistence Background Service is starting on client '{@ClientId}' and group '{@ClientGroup}'",
            machineInstanceInfo.ClientId,
            machineInstanceInfo.ClientGroup
        );

        await ProcessAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "MessagePersistence Background Service is stopping on client '{@ClientId}' and group '{@ClientGroup}'",
            machineInstanceInfo.ClientId,
            machineInstanceInfo.ClientGroup
        );

        return base.StopAsync(cancellationToken);
    }

    private async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = serviceProvider.CreateAsyncScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();
                await service.ProcessAllAsync(stoppingToken);
            }

            var delay = _options.Interval is { }
                ? TimeSpan.FromSeconds((int)_options.Interval)
                : TimeSpan.FromSeconds(30);

            await Task.Delay(delay, stoppingToken);
        }
    }
}
