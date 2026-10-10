using FoodDelivery.Services.Orders.Orders.Models;
using Microsoft.EntityFrameworkCore;
using Squirrel.Abstractions.Persistence.EfCore;

namespace FoodDelivery.Services.Orders.Shared.Contracts;

public interface IOrdersDbContext : IDbContext
{
    public DbSet<Order> Orders { get; }
}
