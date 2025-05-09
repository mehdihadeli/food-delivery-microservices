using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages.MessagePersistence;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Messaging.Persistence.Postgres.Extensions;

public static class DependencyInjectionExtensions
{
    public static void AddPostgresMessagePersistence(
        this IServiceCollection services,
        Action<MessagePersistenceOptions>? configurator = null
    )
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // add an option to the dependency injection
        services.AddValidationOptions(configurator: configurator);

        services.TryAddScoped<IMessagePersistenceConnectionFactory>(sp =>
        {
            var messagePersistenceOptions = sp.GetRequiredService<IOptions<MessagePersistenceOptions>>().Value;
            return new NpgsqlMessagePersistenceConnectionFactory(
                messagePersistenceOptions.ConnectionString.NotBeEmptyOrNull()
            );
        });

        services.AddDbContext<MessagePersistenceDbContext>(
            (sp, opt) =>
            {
                var messagePersistenceOptions = sp.GetRequiredService<IOptions<MessagePersistenceOptions>>().Value;
                opt.UseNpgsql(
                        messagePersistenceOptions.ConnectionString,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(
                                messagePersistenceOptions.MigrationAssembly
                                    ?? typeof(MessagePersistenceDbContext).Assembly.GetName().Name
                            );
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    .UseSnakeCaseNamingConvention();
            }
        );

        // replace the default in-memory message persistence repository with the postgres one
        services.Replace(
            ServiceDescriptor.Scoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>()
        );
    }
}
