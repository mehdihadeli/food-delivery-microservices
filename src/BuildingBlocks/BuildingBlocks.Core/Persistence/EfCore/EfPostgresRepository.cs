using System.Diagnostics.CodeAnalysis;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Core.Persistence.EfCore;

public class EfRepository<TDbContext, TEntity, TKey> : EfRepositoryBase<TDbContext, TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    public EfRepository(TDbContext dbContext, IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
        : base(dbContext, aggregatesDomainEventsStore)
    {
    }
}

public class EfRepository<TDbContext, TEntity> : EfRepository<TDbContext, TEntity, Guid>
    where TEntity : class, IHaveIdentity<Guid>
    where TDbContext : DbContext
{
    public EfRepository(TDbContext dbContext, [NotNull] IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore)
        : base(dbContext, aggregatesDomainEventsStore)
    {
    }
}
