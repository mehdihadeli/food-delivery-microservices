using BuildingBlocks.Core.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using ECommerce.Services.Customers.Customers.Models;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Contracts;

namespace ECommerce.Services.Customers.Shared.Data;

public class CustomersDbContext : EfDbContextBase, ICustomersDbContext
{
    public const string DefaultSchema = "customer";

    public CustomersDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RestockSubscription> RestockSubscriptions => Set<RestockSubscription>();
}
