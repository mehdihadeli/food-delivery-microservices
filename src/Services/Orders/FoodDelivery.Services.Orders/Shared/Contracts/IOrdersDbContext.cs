using BuildingBlocks.Abstractions.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Services.Orders.Orders.Models;

namespace FoodDelivery.Services.Orders.Shared.Contracts;

public interface IOrdersDbContext : IDbContext
{
    public DbSet<Order> Orders { get; }
}
