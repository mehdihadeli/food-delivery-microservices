using BuildingBlocks.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace BuildingBlocks.Core.Persistence.EfCore;

public class EfRepository<TDbContext, TEntity, TKey>(TDbContext dbContext, ISieveProcessor sieveProcessor)
    : EfRepositoryBase<TDbContext, TEntity, TKey>(dbContext, sieveProcessor)
    where TEntity : class, IEntity<TKey>
    where TDbContext : DbContext;

public class EfRepository<TDbContext, TEntity>(TDbContext dbContext, ISieveProcessor sieveProcessor)
    : EfRepository<TDbContext, TEntity, Guid>(dbContext, sieveProcessor)
    where TEntity : class, IEntity<Guid>
    where TDbContext : DbContext;
