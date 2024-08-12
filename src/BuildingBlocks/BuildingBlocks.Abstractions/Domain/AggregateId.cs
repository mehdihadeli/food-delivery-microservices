namespace BuildingBlocks.Abstractions.Domain;

public record AggregateId<T> : Identity<T>
{
    // EF
    protected AggregateId(T value)
    {
        Value = value;
    }

    public static implicit operator T(AggregateId<T> id)
    {
        return id.Value;
    }

    // validations should be placed here instead of constructor
    public static AggregateId<T> CreateAggregateId(T id)
    {
        return new AggregateId<T>(id);
    }
}

public record AggregateId : AggregateId<long>
{
    // EF
    protected AggregateId(long value)
        : base(value) { }

    // validations should be placed here instead of constructor
    public new static AggregateId CreateAggregateId(long value)
    {
        return new AggregateId(value);
    }

    public static implicit operator long(AggregateId id)
    {
        return id.Value;
    }
}
