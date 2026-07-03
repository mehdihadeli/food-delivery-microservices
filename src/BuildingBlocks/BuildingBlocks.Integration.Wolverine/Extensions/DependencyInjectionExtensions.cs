using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Abstractions.Messages.MessagePersistence;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.HostApplicationBuilderExtensions;
using BuildingBlocks.Core.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Core.Web.Extensions;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Constants = BuildingBlocks.Core.Constants;

namespace BuildingBlocks.Integration.Wolverine.Extensions;

public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddWolverineEventBus(
        this IHostApplicationBuilder builder,
        Action<WolverineOptions>? configureMessagesTopologies = null,
        Action<WolverineOptions>? configureBusRegistration = null,
        Action<WolverineBusOptions>? configureWolverineBusOptions = null,
        string? durabilityConnectionStringName = null,
        Assembly[]? assemblies = null
    )
    {
        assemblies ??= [Assembly.GetCallingAssembly()];

        // Add option to the dependency injection
        builder.Services.AddValidationOptions(configurator: configureWolverineBusOptions);
        var wolverineBusOptions = builder.Configuration.BindOptions<WolverineBusOptions>();

        // - EnvironmentVariablesConfigurationProvider is injected by aspire and use to read configuration values from environment variables with `ConnectionStrings:redis` key on configuration.
        // The configuration provider handles these conversions automatically, and `__ (double underscore)` becomes `:` for nested sections, so environment configuration reads its data from the ` ConnectionStrings__redis ` environment. all envs are available in `Environment.GetEnvironmentVariables()`.
        // - For setting none sensitive configuration, we can use Aspire named configuration `Aspire:StackExchange:Redis:DisableHealthChecks` which is of type ConfigurationProvider and should be set in appsetting.json

        // https://learn.microsoft.com/en-us/dotnet/aspire/messaging/rabbitmq-integration?tabs=dotnet-cli#use-a-connection-strin
        // first read from aspire injected ConnectionString then read from config
        var connectionString =
            builder.Configuration.GetConnectionString(Constants.AspireResources.Rabbitmq)
            ?? wolverineBusOptions.RabbitMQConnectionString
            ?? throw new InvalidOperationException(
                $"Connection string '{Constants.AspireResources.Rabbitmq}' or `{nameof(WolverineBusOptions)}.{nameof(WolverineBusOptions.RabbitMQConnectionString)}` not found."
            );

        builder
            .Services.AddOptions<HostOptions>()
            .Configure(options =>
            {
                options.StartupTimeout = TimeSpan.FromSeconds(60);
                options.ShutdownTimeout = TimeSpan.FromSeconds(60);
            });

        builder.Services.AddSingleton<IHostedService>(sp => new RabbitMqTopologyProvisioningHostedService(
            connectionString,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RabbitMqTopologyProvisioningHostedService>>()
        ));

        // will override default null messaging types - we should not use `TryAddTransient` to replace null types
        builder.Services.Replace(ServiceDescriptor.Transient<IExternalEventBus, WolverineEventBus>());
        builder.Services.Replace(ServiceDescriptor.Transient<IBusDirectPublisher, WolverineDirectPublisher>());
        builder.Services.Replace(
            ServiceDescriptor.Scoped<IMessagePersistenceService, WolverineMessagePersistenceService>()
        );

        ConfigureInstrumentation(builder, wolverineBusOptions, connectionString);

        builder.Services.AddWolverine(options =>
        {
            foreach (var assembly in assemblies)
            {
                options.Discovery.IncludeAssembly(assembly);
            }

            options.UseRuntimeCompilation();
            options.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;

            configureBusRegistration?.Invoke(options);

            ConfigureDurability(builder, options, wolverineBusOptions, durabilityConnectionStringName);

            options.UseRabbitMq(connectionString);

            configureMessagesTopologies?.Invoke(options);
        });

        return builder;
    }

    private static void ConfigureDurability(
        IHostApplicationBuilder builder,
        WolverineOptions options,
        WolverineBusOptions wolverineBusOptions,
        string? durabilityConnectionStringName
    )
    {
        if (!wolverineBusOptions.EnableDurability)
        {
            return;
        }

        var connectionString = !string.IsNullOrWhiteSpace(durabilityConnectionStringName)
            ? builder.Configuration.GetConnectionString(durabilityConnectionStringName)
            : null;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{durabilityConnectionStringName}' is required when Wolverine durability is enabled."
            );
        }

        options.PersistMessagesWithPostgresql(connectionString);

        if (wolverineBusOptions.UseEntityFrameworkCoreTransactions)
        {
            options.UseEntityFrameworkCoreTransactions();
        }

        if (wolverineBusOptions.UseDurableLocalQueues)
        {
            options.Policies.UseDurableLocalQueues();
        }
    }

    private static void ConfigureInstrumentation(
        IHostApplicationBuilder builder,
        WolverineBusOptions wolverineBusOptions,
        string connectionString
    )
    {
        if (!wolverineBusOptions.DisableTracing)
        {
            builder
                .Services.AddOpenTelemetry()
                .WithTracing(p =>
                {
                    p.AddSource("RabbitMQ.Client.*");
                });
        }
    }
}
