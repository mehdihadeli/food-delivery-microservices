namespace BuildingBlocks.Abstractions.Domain;

public record EntityId<T> : Identity<T>
{
    // EF
    protected EntityId(T value)
    {
        Value = value;
    }

    public static implicit operator T(EntityId<T> id)
    {
        ArgumentNullException.ThrowIfNull(id.Value);
        return id.Value;
    }

    public static EntityId<T> Of(T id)
    {
        return new EntityId<T>(id);
    }
}

public record EntityId : EntityId<long>
{
    protected EntityId(long value)
        : base(value) { }

    public static implicit operator long(EntityId id)
    {
        return id.Value;
    }

    public static new EntityId Of(long id)
    {
        return new EntityId(id);
    }
}
