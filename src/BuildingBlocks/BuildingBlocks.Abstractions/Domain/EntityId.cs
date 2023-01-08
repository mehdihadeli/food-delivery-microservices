using Ardalis.GuardClauses;

namespace BuildingBlocks.Abstractions.Domain;

public record EntityId<T> : Identity<T>
{
    public static implicit operator T(EntityId<T> id) => Guard.Against.Null(id.Value, nameof(id.Value));
    public static EntityId<T> CreateEntityId(T id) => new() {Value = id};
}

public record EntityId : EntityId<long>
{
    public static implicit operator long(EntityId id) => Guard.Against.Null(id.Value, nameof(id.Value));
    public static new EntityId CreateEntityId(long id) => new() {Value = id};
}
