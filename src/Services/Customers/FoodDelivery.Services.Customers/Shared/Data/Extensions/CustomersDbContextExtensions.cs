using FoodDelivery.Services.Customers.Customers.Models;
using FoodDelivery.Services.Customers.Customers.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Services.Customers.Shared.Data.Extensions;

public static class CustomersDbContextExtensions
{
    public static ValueTask<Customer?> FindCustomerByIdAsync(this CustomersDbContext context, CustomerId id)
    {
        return context.Customers.FindAsync(id);
    }

    public static Task<bool> ExistsCustomerByIdAsync(this CustomersDbContext context, CustomerId id)
    {
        return context.Customers.AnyAsync(x => x.Id == id);
    }
}
