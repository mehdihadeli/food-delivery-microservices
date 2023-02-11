using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Core.Web.Extenions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Core.Messaging.BackgroundServices;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
public class MessagePersistenceBackgroundService : BackgroundService
{
    private readonly ILogger<MessagePersistenceBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly MessagePersistenceOptions _options;
    private readonly IMachineInstanceInfo _machineInstanceInfo;

    private Task? _executingTask;

    public MessagePersistenceBackgroundService(
        ILogger<MessagePersistenceBackgroundService> logger,
        IOptions<MessagePersistenceOptions> options,
        IServiceProvider serviceProvider,
        IHostApplicationLifetime lifetime,
        IMachineInstanceInfo machineInstanceInfo
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _options = options.Value;
        _machineInstanceInfo = machineInstanceInfo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await _lifetime.WaitForAppStartup(stoppingToken))
        {
            return;
        }

        _logger.LogInformation(
            "MessagePersistence Background Service is starting on client '{@ClientId}' and group '{@ClientGroup}'",
            _machineInstanceInfo.ClientId,
            _machineInstanceInfo.ClientGroup
        );

        await ProcessAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "MessagePersistence Background Service is stopping on client '{@ClientId}' and group '{@ClientGroup}'",
            _machineInstanceInfo.ClientId,
            _machineInstanceInfo.ClientGroup
        );

        return base.StopAsync(cancellationToken);
    }

    private async Task ProcessAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using (var scope = _serviceProvider.CreateAsyncScope())
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
