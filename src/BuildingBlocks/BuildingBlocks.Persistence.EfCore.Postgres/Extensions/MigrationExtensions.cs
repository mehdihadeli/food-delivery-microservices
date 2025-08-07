using BuildingBlocks.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public static class MigrationExtensions
{
    public static readonly string ActivitySourceName = "DbMigrations";

    public static IHostApplicationBuilder AddMigration<TContext>(this IHostApplicationBuilder builder)
        where TContext : DbContext => builder.AddMigration<TContext>((_, _) => Task.CompletedTask);

    public static IHostApplicationBuilder AddMigration<TContext>(
        this IHostApplicationBuilder builder,
        Func<TContext, IServiceProvider, Task> seeder
    )
        where TContext : DbContext
    {
        builder.Services.AddScoped<IDataSeeder<TContext>>(sp => new DefaultDataSeeder<TContext>(sp, seeder));
        // Enable migration tracing
        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        builder.Services.AddHostedService<MigrationSeedWorker<TContext>>();

        return builder;
    }

    public static IHostApplicationBuilder AddMigration<TContext, TDbSeeder>(this IHostApplicationBuilder builder)
        where TContext : DbContext
        where TDbSeeder : class, IDataSeeder<TContext>
    {
        builder.Services.AddScoped<IDataSeeder<TContext>, TDbSeeder>();
        // Enable migration tracing
        builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddSource(ActivitySourceName));

        builder.Services.AddHostedService<MigrationSeedWorker<TContext>>();

        return builder;
    }
}

internal class DefaultDataSeeder<TContext>(IServiceProvider sp, Func<TContext, IServiceProvider, Task> seeder)
    : IDataSeeder<TContext>
    where TContext : DbContext
{
    public async Task SeedAsync(TContext context)
    {
        await seeder(context, sp);
    }
}
