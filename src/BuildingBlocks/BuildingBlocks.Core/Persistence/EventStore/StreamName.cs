using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Abstractions.Domain.EventSourcing;
using BuildingBlocks.Core.Extensions;

namespace BuildingBlocks.Core.Persistence.EventStore;

public class StreamName
{
    public string Value { get; }

    public StreamName([NotNull] string? value)
    {
        Value = value.NotBeNull();
    }

    public static StreamName For<T>(string id) => new($"{typeof(T).Name}-{id.NotBeNullOrWhiteSpace()}");

    public static StreamName For<TAggregate, TId>(TId aggregateId)
        where TAggregate : IEventSourcedAggregate<TId>
    {
        aggregateId.NotBeNull();
        var id = aggregateId.ToString().NotBeNullOrWhiteSpace();
        return For<TAggregate>(id);
    }

    public static implicit operator string(StreamName streamName) => streamName.Value;

    public override string ToString() => Value;
}
