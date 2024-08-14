using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Services.Customers.Customers.Models;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Services.Customers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.Shared.Data;

public class CustomersDbContext(DbContextOptions<CustomersDbContext> options)
    : EfDbContextBase(options),
        ICustomersDbContext
{
    public const string DefaultSchema = "customer";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public virtual DbSet<Customer> Customers { get; set; } = default!;
    public virtual DbSet<RestockSubscription> RestockSubscriptions { get; set; } = default!;
}
