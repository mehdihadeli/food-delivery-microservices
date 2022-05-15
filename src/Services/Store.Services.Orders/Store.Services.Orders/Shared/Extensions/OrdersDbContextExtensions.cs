using Microsoft.EntityFrameworkCore;
using Store.Services.Orders.Orders.Models;
using Store.Services.Orders.Orders.ValueObjects;
using Store.Services.Orders.Shared.Data;

namespace Store.Services.Orders.Shared.Extensions;

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
