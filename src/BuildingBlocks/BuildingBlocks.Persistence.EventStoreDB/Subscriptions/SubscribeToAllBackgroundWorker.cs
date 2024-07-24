using BuildingBlocks.Abstractions.Events;
using BuildingBlocks.Abstractions.Persistence.EventStore.Projections;
using BuildingBlocks.Core.Threading;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Persistence.EventStoreDB.Extensions;
using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Persistence.EventStoreDB.Subscriptions;

// Ref: https://github.com/oskardudycz/EventSourcing.NetCore/
public class EventStoreDBSubscriptionToAll : BackgroundService
{
    private readonly EventStoreDbOptions _eventStoreDbOptions;
    private readonly EventStoreClient _eventStoreClient;
    private readonly IReadProjectionPublisher _projectionPublisher;
    private readonly IInternalEventBus _internalEventBus;
    private readonly ISubscriptionCheckpointRepository _checkpointRepository;
    private readonly ILogger<EventStoreDBSubscriptionToAll> _logger;
    private readonly object _resubscribeLock = new();
    private CancellationToken _cancellationToken;

    public EventStoreDBSubscriptionToAll(
        IOptions<EventStoreDbOptions> eventStoreDbOptions,
        EventStoreClient eventStoreClient,
        IReadProjectionPublisher projectionPublisher,
        IInternalEventBus internalEventBus,
        ISubscriptionCheckpointRepository checkpointRepository,
        ILogger<EventStoreDBSubscriptionToAll> logger
    )
    {
        _eventStoreDbOptions = eventStoreDbOptions.Value;
        _eventStoreClient = eventStoreClient;
        _projectionPublisher = projectionPublisher;
        _internalEventBus = internalEventBus;
        _checkpointRepository = checkpointRepository;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancellationToken = stoppingToken;

        return SubscribeToAll(stoppingToken);
    }

    private async Task SubscribeToAll(CancellationToken cancellationToken = default)
    {
        // see: https://github.com/dotnet/runtime/issues/36063
        await Task.Yield();

        _logger.LogInformation(
            "Subscription to all '{SubscriptionId}'",
            _eventStoreDbOptions.SubscriptionOptions.SubscriptionId
        );

        var checkpoint = await _checkpointRepository.Load(
            _eventStoreDbOptions.SubscriptionOptions.SubscriptionId,
            cancellationToken
        );

        await _eventStoreClient.SubscribeToAllAsync(
            checkpoint == null ? FromAll.Start : FromAll.After(new Position(checkpoint.Value, checkpoint.Value)),
            HandleEvent,
            _eventStoreDbOptions.SubscriptionOptions.ResolveLinkTos,
            HandleDrop,
            new(EventTypeFilter.ExcludeSystemEvents()),
            default,
            cancellationToken
        );

        _logger.LogInformation(
            "Subscription to all '{SubscriptionId}' started",
            _eventStoreDbOptions.SubscriptionOptions.SubscriptionId
        );
    }

    private async Task HandleEvent(
        StreamSubscription subscription,
        ResolvedEvent resolvedEvent,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent))
                return;

            var streamEvent = resolvedEvent.ToStreamEvent();

            if (streamEvent == null)
            {
                // That can happen if we're sharing database between modules.
                // If we're subscribing to all and not filtering out events from other modules,
                // then we might get events that are from other module and we might not be able to deserialize them.
                // In that case it's safe to ignore deserialization error.
                // You may add more sophisticated logic checking if it should be ignored or not.
                _logger.LogWarning("Couldn't deserialize event with id: {EventId}", resolvedEvent.Event.EventId);

                if (!_eventStoreDbOptions.SubscriptionOptions.IgnoreDeserializationErrors)
                {
                    throw new InvalidOperationException(
                        $"Unable to deserialize event {resolvedEvent.Event.EventType} with id: {resolvedEvent.Event.EventId}"
                    );
                }

                return;
            }

            // publish event to internal event bus
            await _internalEventBus.Publish(streamEvent, cancellationToken);

            await _projectionPublisher.PublishAsync(streamEvent, cancellationToken);

            await _checkpointRepository.Store(
                _eventStoreDbOptions.SubscriptionOptions.SubscriptionId,
                resolvedEvent.Event.Position.CommitPosition,
                cancellationToken
            );
        }
        catch (Exception e)
        {
            _logger.LogError(
                "Error consuming message: {ExceptionMessage}{ExceptionStackTrace}",
                e.Message,
                e.StackTrace
            );

            // if you're fine with dropping some events instead of stopping subscription
            // then you can add some logic if error should be ignored
            throw;
        }
    }

    private void HandleDrop(
        StreamSubscription streamSubscription,
        SubscriptionDroppedReason reason,
        Exception? exception
    )
    {
        _logger.LogError(
            exception,
            "Subscription to all '{SubscriptionId}' dropped with '{Reason}'",
            _eventStoreDbOptions.SubscriptionOptions.SubscriptionId,
            reason
        );

        if (exception is RpcException { StatusCode: StatusCode.Cancelled })
            return;

        Resubscribe();
    }

    private void Resubscribe()
    {
        // You may consider adding a max resubscribe count if you want to fail process
        // instead of retrying until database is up
        while (true)
        {
            var resubscribed = false;
            try
            {
                Monitor.Enter(_resubscribeLock);

                // No synchronization consumeContext is needed to disable synchronization consumeContext.
                // That enables running asynchronous method not causing deadlocks.
                // As this is a background process then we don't need to have async consumeContext here.
                using (NoSynchronizationContextScope.Enter())
                {
                    SubscribeToAll(_cancellationToken).Wait(_cancellationToken);
                }

                resubscribed = true;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to resubscribe to all '{SubscriptionId}' dropped with '{ExceptionMessage}{ExceptionStackTrace}'",
                    _eventStoreDbOptions.SubscriptionOptions.SubscriptionId,
                    exception.Message,
                    exception.StackTrace
                );
            }
            finally
            {
                Monitor.Exit(_resubscribeLock);
            }

            if (resubscribed)
                break;

            // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
            // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
            Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));
        }
    }

    private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.Data.Length != 0)
            return false;

        _logger.LogInformation("Event without data received");
        return true;
    }

    private bool IsCheckpointEvent(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.EventType != TypeMapper.GetTypeName<CheckpointStored>())
            return false;

        _logger.LogInformation("Checkpoint event - ignoring");
        return true;
    }
}
