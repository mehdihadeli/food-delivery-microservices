using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Core.Domain;

public abstract class AuditAggregate<TId> : Aggregate<TId>, IAuditableEntity<TId>
{
    public DateTime? LastModified { get; protected set; }
    public int? LastModifiedBy { get; protected set; }
}

public abstract class AuditAggregate<TIdentity, TId> : AuditAggregate<TIdentity>
    where TIdentity : Identity<TId>
{
}

public abstract class AuditAggregate : AuditAggregate<Identity<long>, long>
{
}
