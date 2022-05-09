namespace BuildingBlocks.Abstractions.Domain;

public interface IAuditableEntity<out TId> : IEntity<TId>, IHaveAudit
{
}

public interface IAuditableEntity<out TIdentity, TId> : IAuditableEntity<TIdentity>
    where TIdentity : Identity<TId>
{
}

public interface IAuditableEntity : IAuditableEntity<Identity<long>, long>
{
}
