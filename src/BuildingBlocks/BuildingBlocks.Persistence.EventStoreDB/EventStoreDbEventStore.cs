using System.Collections.Immutable;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Persistence.EventStore;
using BuildingBlocks.Persistence.EventStoreDB.Extensions;
using EventStore.Client;

namespace BuildingBlocks.Persistence.EventStoreDB;

// https://developers.eventstore.com/clients/dotnet/21.2/migration-to-gRPC.html
public class EventStoreDbEventStore(EventStoreClient grpcClient) : IEventStore
{
    public async Task<bool> StreamExists(string streamId, CancellationToken cancellationToken = default)
    {
        var read = grpcClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            StreamPosition.Start,
            1,
            cancellationToken: cancellationToken
        );

        var state = await read.ReadState;
        return state == ReadState.Ok;
    }

    public async Task<IEnumerable<IStreamEventEnvelopeBase>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        int maxCount = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        var readResult = grpcClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            fromVersion?.AsStreamPosition() ?? StreamPosition.Start,
            maxCount,
            cancellationToken: cancellationToken
        );

        var resolvedEvents = await readResult.ToListAsync(cancellationToken);

        return resolvedEvents.ToStreamEvents();
    }

    public Task<IEnumerable<IStreamEventEnvelopeBase>> GetStreamEventsAsync(
        string streamId,
        StreamReadPosition? fromVersion = null,
        CancellationToken cancellationToken = default
    )
    {
        return GetStreamEventsAsync(streamId, fromVersion, int.MaxValue, cancellationToken);
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEventEnvelopeBase @event,
        CancellationToken cancellationToken = default
    )
    {
        return AppendEventsAsync(
            streamId,
            new List<IStreamEventEnvelopeBase> { @event }.ToImmutableList(),
            ExpectedStreamVersion.NoStream,
            cancellationToken
        );
    }

    public Task<AppendResult> AppendEventAsync(
        string streamId,
        IStreamEventEnvelopeBase @event,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        return AppendEventsAsync(
            streamId,
            new List<IStreamEventEnvelopeBase> { @event }.ToImmutableList(),
            expectedRevision,
            cancellationToken
        );
    }

    public async Task<AppendResult> AppendEventsAsync(
        string streamId,
        IReadOnlyCollection<IStreamEventEnvelopeBase> events,
        ExpectedStreamVersion expectedRevision,
        CancellationToken cancellationToken = default
    )
    {
        var eventsData = events.Select(x => x.ToJsonEventData());

        if (expectedRevision == ExpectedStreamVersion.NoStream)
        {
            var result = await grpcClient.AppendToStreamAsync(
                streamId,
                StreamState.NoStream,
                eventsData,
                cancellationToken: cancellationToken
            );

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64()
            );
        }

        if (expectedRevision == ExpectedStreamVersion.Any)
        {
            var result = await grpcClient.AppendToStreamAsync(
                streamId,
                StreamState.Any,
                eventsData,
                cancellationToken: cancellationToken
            );

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64()
            );
        }
        else
        {
            var result = await grpcClient.AppendToStreamAsync(
                streamId,
                expectedRevision.AsStreamRevision(),
                eventsData,
                cancellationToken: cancellationToken
            );

            return new AppendResult(
                (long)result.LogPosition.CommitPosition,
                result.NextExpectedStreamRevision.ToInt64()
            );
        }
    }

    public async Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        StreamReadPosition fromVersion,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        var readResult = grpcClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            fromVersion.AsStreamPosition(),
            cancellationToken: cancellationToken
        );

        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
            return null;

        // var streamEvents = (await GetStreamEventsAsync(streamId, fromVersion, int.MaxValue, cancellationToken)).Select(x => x.Data);
        return await readResult
            .Select(@event => @event.DeserializeData()!)
            .AggregateAsync(
                defaultAggregateState,
                (agg, @event) =>
                {
                    fold(@event);
                    return agg;
                },
                cancellationToken
            );
    }

    public Task<TAggregate?> AggregateStreamAsync<TAggregate, TId>(
        string streamId,
        TAggregate defaultAggregateState,
        Action<object> fold,
        CancellationToken cancellationToken = default
    )
        where TAggregate : class, IEventSourcedAggregate<TId>, new()
    {
        return AggregateStreamAsync<TAggregate, TId>(
            streamId,
            StreamReadPosition.Start,
            defaultAggregateState,
            fold,
            cancellationToken
        );
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
