using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain.EventSourcing;

namespace BuildingBlocks.Core.Persistence.EventStore;

public class StreamName
{
    public string Value { get; }

    public StreamName(string value)
    {
        Guard.Against.NullOrEmpty(value, nameof(value));
        Value = value;
    }

    public static StreamName For<T>(string id) => new($"{typeof(T).Name}-{Guard.Against.NullOrEmpty(id, nameof(id))}");

    public static StreamName For<TAggregate, TId>(TId aggregateId)
        where TAggregate : IEventSourcedAggregate<TId>
        => For<TAggregate>(aggregateId.ToString());

    public static implicit operator string(StreamName streamName) => streamName.Value;

    public override string ToString() => Value;
}
