using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Messages.MessagePersistence;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Messaging.Persistence.Postgres;

public static class DependencyInjectionExtensions
{
    public static void AddPostgresMessagePersistence(
        this IHostApplicationBuilder builder,
        string? connectionStringName = null,
        Action<MessagePersistenceOptions>? configurator = null
    )
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // add an option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configurator);

        var postgresOptions = builder.Configuration.BindOptions<MessagePersistenceOptions>();

        // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:pg-catalogsdb` key on configuration.
        // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections,
        // so environment configuration reads its data from the ` ConnectionStrings__pg-catalogsdb ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
        // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration?tabs=dotnet-cli#use-a-connection-string
        // first read from aspire injected ConnectionString then read from config
        var connectionString =
            !string.IsNullOrWhiteSpace(connectionStringName)
            && !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString(connectionStringName))
                ? builder.Configuration.GetConnectionString(connectionStringName)
                : postgresOptions.ConnectionString
                    ?? throw new InvalidOperationException(
                        $"Postgres connection string '{connectionStringName}' or `postgresOptions.ConnectionString` not found."
                    );

        builder.Services.AddScoped<IMessagePersistenceConnectionFactory>(
            sp => new NpgsqlMessagePersistenceConnectionFactory(connectionString!)
        );

        builder.Services.AddDbContext<MessagePersistenceDbContext>(
            (sp, opt) =>
            {
                opt.UseNpgsql(
                        connectionString,
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly(
                                postgresOptions.MigrationAssembly
                                    ?? typeof(MessagePersistenceDbContext).Assembly.GetName().Name
                            );
                            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                        }
                    )
                    .UseSnakeCaseNamingConvention();
            }
        );

        // https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-entity-framework-integration?tabs=dotnet-cli#enrich-an-npgsql-database-context
        // For config health check and instrumentation for postgres dbcontext
        builder.EnrichNpgsqlDbContext<MessagePersistenceDbContext>();

        builder.AddMigration<MessagePersistenceDbContext>();

        // replace the default in-memory message persistence repository with the postgres one
        builder.Services.Replace(
            ServiceDescriptor.Scoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>()
        );
    }
}
