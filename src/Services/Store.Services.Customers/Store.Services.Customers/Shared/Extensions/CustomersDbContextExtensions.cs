using Store.Services.Customers.Customers.Models;
using Store.Services.Customers.Customers.ValueObjects;
using Store.Services.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace Store.Services.Customers.Shared.Extensions;

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
