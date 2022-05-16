using Microsoft.EntityFrameworkCore;
using ECommerce.Services.Orders.Orders.Models;
using ECommerce.Services.Orders.Orders.ValueObjects;
using ECommerce.Services.Orders.Shared.Data;

namespace ECommerce.Services.Orders.Shared.Extensions;

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
