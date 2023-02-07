using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Core.Domain;

public abstract class Entity<TId> : IEntity<TId>
{
    public TId Id { get; protected init; } = default!;
    public DateTime Created { get; private set; } = default!;
    public int? CreatedBy { get; private set; } = default!;
}

public abstract class Entity<TIdentity, TId> : Entity<TIdentity>
    where TIdentity : Identity<TId>
{
}

public abstract class Entity : Entity<EntityId, long>, IEntity
{
}
