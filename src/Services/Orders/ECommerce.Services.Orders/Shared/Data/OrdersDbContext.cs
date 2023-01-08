using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Services.Orders.Orders.Models;
using ECommerce.Services.Orders.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Orders.Shared.Data;

public class OrdersDbContext : EfDbContextBase, IOrdersDbContext
{
    public const string DefaultSchema = "order";

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<Order> Orders => Set<Order>();
}
