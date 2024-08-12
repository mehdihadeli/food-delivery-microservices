using FoodDelivery.Services.Orders.Orders.Models;
using FoodDelivery.Services.Orders.Orders.ValueObjects;
using FoodDelivery.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Orders.Shared.Extensions;

public static class OrdersDbContextExtensions
{
    public static ValueTask<Order?> FindOrderByIdAsync(this OrdersDbContext context, OrderId id)
    {
        return context.Orders.FindAsync(id);
    }

    public static Task<bool> ExistsOrderByIdAsync(this OrdersDbContext context, OrderId id)
    {
        return context.Orders.AnyAsync(x => x.Id == id);
    }
}
