using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using ECommerce.Services.Orders.Orders.Models;

namespace ECommerce.Services.Orders.Shared.Contracts;

public interface IOrdersDbContext : IDbContext
{
    public DbSet<Order> Orders { get; }
}
