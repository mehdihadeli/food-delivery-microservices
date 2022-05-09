using Ardalis.GuardClauses;

namespace BuildingBlocks.Abstractions.Domain;

public record AggregateId<T> : Identity<T>
{
    public AggregateId(T value) : base(value)
    {
    }

    public static implicit operator T(AggregateId<T> id) => Guard.Against.Null(id.Value, nameof(id.Value));
    public static implicit operator AggregateId<T>(T id) => new(id);
}

public record AggregateId : AggregateId<long>
{
    public AggregateId(long value) : base(value)
    {
        Guard.Against.NegativeOrZero(value, nameof(value));
    }

    public static implicit operator long(AggregateId id) => Guard.Against.Null(id.Value, nameof(id.Value));
    public static implicit operator AggregateId(long id) => new(id);
}
