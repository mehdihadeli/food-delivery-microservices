using Ardalis.GuardClauses;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging.Persistence.Postgres.Extensions;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Validation;
using ECommerce.Services.Customers.Customers;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.RestockSubscriptions;
using ECommerce.Services.Customers.Users;

namespace ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        SnowFlakIdGenerator.Configure(2);

        services.AddCore(configuration);

        services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            Guard.Against.Null(postgresOptions, nameof(postgresOptions));

            healthChecksBuilder.AddNpgSql(
                postgresOptions.ConnectionString,
                name: "Customers-Postgres-Check",
                tags: new[] {"customers-postgres"});
        });

        services.AddEmailService(configuration);

        services.AddCqrs(
            doMoreActions: s =>
            {
                s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                    .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                    .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                    .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                    .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                    .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                    .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                    .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
            });

        services.AddPostgresMessagePersistence(configuration);

        services.AddCustomMassTransit(
            configuration,
            webHostEnvironment,
            (context, cfg) =>
            {
                cfg.AddUsersEndpoints(context);
                cfg.AddProductEndpoints(context);

                cfg.AddCustomerPublishers();
                cfg.AddRestockSubscriptionPublishers();
            },
            autoConfigEndpoints: false);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        services.AddCustomHttpClients(configuration);

        return services;
    }
}
