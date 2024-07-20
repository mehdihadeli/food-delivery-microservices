using FoodDelivery.Services.Customers.Customers.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.Shared.Contracts;

public interface ICustomersDbContext
{
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    public DbSet<Customer> Customers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
