using System.Reflection;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using BuildingBlocks.Messaging.Persistence.Postgres.MessagePersistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Messaging.Persistence.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddPostgresMessagePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var option = configuration.GetOptions<MessagePersistenceOptions>(nameof(MessagePersistenceOptions));

        Guard.Against.Null(option, nameof(MessagePersistenceOptions));
        Guard.Against.NullOrEmpty(option.ConnectionString, nameof(option.ConnectionString));

        services.AddDbContext<MessagePersistenceDbContext>(options =>
        {
            options.UseNpgsql(option.ConnectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            }).UseSnakeCaseNamingConvention();
        });

        services.ReplaceScoped<IMessagePersistenceRepository, PostgresMessagePersistenceRepository>();
    }
}
