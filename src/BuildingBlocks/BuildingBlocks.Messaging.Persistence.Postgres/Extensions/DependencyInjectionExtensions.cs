using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Messaging.Persistence.Postgres.Extensions;

public static class DependencyInjectionExtensions
{
    public static void AddPostgresMessagePersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MessagePersistenceOptions>? configurator = null
    )
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var options = configuration.BindOptions<MessagePersistenceOptions>();
        configurator?.Invoke(options);

        // add option to the dependency injection
        services.AddValidationOptions<MessagePersistenceOptions>(opt => configurator?.Invoke(opt));

        services.TryAddScoped<IMessagePersistenceConnectionFactory>(sp => new NpgsqlMessagePersistenceConnectionFactory(
            options.ConnectionString.NotBeEmptyOrNull()
        ));

        services.AddDbContext<MessagePersistenceDbContext>(
            (sp, opt) =>
            {
                opt.UseNpgsql(
                        options.ConnectionString,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(
                                options.MigrationAssembly ?? typeof(MessagePersistenceDbContext).Assembly.GetName().Name
                            );
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    .UseSnakeCaseNamingConvention();
            }
        );

        services.Replace(
            ServiceDescriptor.Scoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>()
        );
    }
}
