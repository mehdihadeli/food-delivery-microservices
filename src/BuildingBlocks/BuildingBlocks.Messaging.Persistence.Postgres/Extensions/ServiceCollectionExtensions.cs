using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Messaging.Persistence.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddPostgresMessagePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddScoped<IMessagePersistenceConnectionFactory>(sp =>
        {
            var postgresOptions = sp.GetService<IOptions<MessagePersistenceOptions>>();
            Guard.Against.NullOrEmpty(
                postgresOptions?.Value.ConnectionString,
                nameof(postgresOptions.Value.ConnectionString));
            return new MessagePersistenceConnectionFactory(postgresOptions.Value.ConnectionString);
        });

        services.AddDbContext<MessagePersistenceDbContext>((sp, options) =>
        {
            var connectionFactory = sp.GetRequiredService<IMessagePersistenceConnectionFactory>();
            var conn = connectionFactory.GetOrCreateConnectionAsync().GetAwaiter().GetResult();

            options.UseNpgsql(conn, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();
        });

        services.ReplaceScoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>();
    }
}
