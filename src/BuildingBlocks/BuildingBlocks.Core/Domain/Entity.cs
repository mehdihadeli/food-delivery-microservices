using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Core.Domain;

public abstract class Entity<TId> : IEntity<TId>
{
    protected Entity(TId id) => Id = id;
    protected Entity() { }

    public TId Id { get; protected set; }

    public DateTime Created { get; protected set; }
    public int? CreatedBy { get; protected set; }
}

public abstract class Entity<TIdentity, TId> : Entity<TIdentity>
    where TIdentity : Identity<TId>
{
}

public abstract class Entity : Entity<EntityId, long>, IEntity
{
}
