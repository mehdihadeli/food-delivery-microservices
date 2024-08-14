using BuildingBlocks.Core.Events.Internal;
using BuildingBlocks.Persistence.EventStoreDB.Extensions;
using EventStore.Client;

namespace BuildingBlocks.Persistence.EventStoreDB.Subscriptions;

public record CheckpointStored(string SubscriptionId, ulong? Position, DateTime CheckPointedAt) : DomainEvent;

public class EventStoreDbSubscriptionCheckPointRepository(EventStoreClient eventStoreClient)
    : ISubscriptionCheckpointRepository
{
    private readonly EventStoreClient _eventStoreClient =
        eventStoreClient ?? throw new ArgumentNullException(nameof(eventStoreClient));

    public async ValueTask<ulong?> Load(string subscriptionId, CancellationToken ct)
    {
        var streamName = GetCheckpointStreamName(subscriptionId);

        var result = _eventStoreClient.ReadStreamAsync(
            Direction.Backwards,
            streamName,
            StreamPosition.End,
            1,
            cancellationToken: ct
        );

        if (await result.ReadState == ReadState.StreamNotFound)
        {
            return null;
        }

        ResolvedEvent? @event = await result.FirstOrDefaultAsync(ct);

        return @event?.DeserializeData<CheckpointStored>().Position;
    }

    public async ValueTask Store(string subscriptionId, ulong position, CancellationToken ct)
    {
        var @event = new CheckpointStored(subscriptionId, position, DateTime.UtcNow);
        var eventToAppend = new[] { @event.ToJsonEventData() };
        var streamName = GetCheckpointStreamName(subscriptionId);

        try
        {
            // store new checkpoint expecting stream to exist
            await _eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.StreamExists,
                eventToAppend,
                cancellationToken: ct
            );
        }
        catch (WrongExpectedVersionException)
        {
            // WrongExpectedVersionException means that stream did not exist
            // Set the checkpoint stream to have at most 1 event
            // using stream metadata $maxCount property
            await _eventStoreClient.SetStreamMetadataAsync(
                streamName,
                StreamState.NoStream,
                new StreamMetadata(1),
                cancellationToken: ct
            );

            // append event again expecting stream to not exist
            await _eventStoreClient.AppendToStreamAsync(
                streamName,
                StreamState.NoStream,
                eventToAppend,
                cancellationToken: ct
            );
        }
    }

    private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
}
