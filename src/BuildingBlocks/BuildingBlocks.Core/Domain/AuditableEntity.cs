using BuildingBlocks.Abstractions.Domain;

namespace BuildingBlocks.Core.Domain;

public class AuditableEntity<TId> : Entity<TId>, IAuditableEntity<TId>
{
    public DateTime? LastModified { get; init; } = default!;
    public int? LastModifiedBy { get; init; } = default!;
}

public abstract class AuditableEntity<TIdentity, TId> : AuditableEntity<TIdentity>
    where TIdentity : Identity<TId>;

public class AuditableEntity : AuditableEntity<Identity<long>, long>;
