using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Abstractions.Persistence;

public interface IDataSeeder<in TContext>
    where TContext : DbContext
{
    Task SeedAsync(TContext context);
}
