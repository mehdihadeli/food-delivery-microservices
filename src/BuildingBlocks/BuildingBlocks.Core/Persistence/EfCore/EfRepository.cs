using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Abstractions.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace BuildingBlocks.Core.Persistence.EfCore;

public class EfRepository<TDbContext, TEntity, TKey> : EfRepositoryBase<TDbContext, TEntity, TKey>
    where TEntity : class, IHaveIdentity<TKey>
    where TDbContext : DbContext
{
    public EfRepository(
        TDbContext dbContext,
        ISieveProcessor sieveProcessor,
        IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore
    )
        : base(dbContext, sieveProcessor, aggregatesDomainEventsStore) { }
}

public class EfRepository<TDbContext, TEntity> : EfRepository<TDbContext, TEntity, Guid>
    where TEntity : class, IHaveIdentity<Guid>
    where TDbContext : DbContext
{
    public EfRepository(
        TDbContext dbContext,
        ISieveProcessor sieveProcessor,
        IAggregatesDomainEventsRequestStore aggregatesDomainEventsStore
    )
        : base(dbContext, sieveProcessor, aggregatesDomainEventsStore) { }
}
