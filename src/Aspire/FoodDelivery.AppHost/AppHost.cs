// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/integrations-overview#hosting-integrations
// Hosting integrations configure applications by provisioning resources (like containers or cloud resources) or pointing to existing instances (such as a local SQL server)

using BuildingBlocks.AspireIntegrations.AspireDashBoard;
using BuildingBlocks.AspireIntegrations.ElasticSearch;
using BuildingBlocks.AspireIntegrations.EventStore;
using BuildingBlocks.AspireIntegrations.Grafana;
using BuildingBlocks.AspireIntegrations.HealthChecksUI;
using BuildingBlocks.AspireIntegrations.Jaeger;
using BuildingBlocks.AspireIntegrations.Kibana;
using BuildingBlocks.AspireIntegrations.Loki;
using BuildingBlocks.AspireIntegrations.Mongo;
using BuildingBlocks.AspireIntegrations.OpenTelemetryCollector;
using BuildingBlocks.AspireIntegrations.Postgres;
using BuildingBlocks.AspireIntegrations.Prometheus;
using BuildingBlocks.AspireIntegrations.RabbitMQ;
using BuildingBlocks.AspireIntegrations.Redis;
using BuildingBlocks.AspireIntegrations.Tempo;
using BuildingBlocks.AspireIntegrations.Zipkin;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Core.Extensions;
using FoodDelivery.AppHost.Extensions;
using FoodDelivery.Services.Shared.Constants;
using Microsoft.AspNetCore.Authentication.OAuth;
using Scalar.Aspire;
using OAuthOptions = BuildingBlocks.Core.Security.OAuthOptions;

var builder = DistributedApplication.CreateBuilder(args);

var appHostLaunchProfile = builder.GetLaunchProfileName();
Console.WriteLine($"AppHost LaunchProfile is: {appHostLaunchProfile}");

var pgUser = builder.AddParameter("pg-user", value: "postgres", publishValueAsDefault: true);
var pgPassword = builder.AddParameter(name: "pg-password", value: new GenerateParameterDefault { MinLength = 3 }, true);

var postgres = builder.AddAspirePostgres(
    AspireResources.Postgres,
    userName: pgUser,
    password: pgPassword,
    initScriptPath: "./../../../deployments/configs/init-postgres.sql"
);
var mongo = builder.AddAspireMongoDB(AspireResources.MongoDb);

var catalogsPostgres = postgres.AddAspirePostgresDatabase(
    nameOrConnectionStringName: AspireApplicationResources.PostgresDatabase.Catalogs,
    databaseName: nameof(AspireApplicationResources.PostgresDatabase.Catalogs).ToLowerInvariant()
);
var customersPostgres = postgres.AddAspirePostgresDatabase(
    nameOrConnectionStringName: AspireApplicationResources.PostgresDatabase.Customers,
    databaseName: nameof(AspireApplicationResources.PostgresDatabase.Customers).ToLowerInvariant()
);
var identityPostgres = postgres.AddAspirePostgresDatabase(
    nameOrConnectionStringName: AspireApplicationResources.PostgresDatabase.Identity,
    databaseName: nameof(AspireApplicationResources.PostgresDatabase.Identity).ToLowerInvariant()
);
var ordersPostgres = postgres.AddAspirePostgresDatabase(
    nameOrConnectionStringName: AspireApplicationResources.PostgresDatabase.Orders,
    databaseName: nameof(AspireApplicationResources.PostgresDatabase.Orders).ToLowerInvariant()
);
var healthDb = postgres.AddAspirePostgresDatabase(
    AspireApplicationResources.PostgresDatabase.Health,
    databaseName: nameof(AspireApplicationResources.PostgresDatabase.Health).ToLowerInvariant()
);

var catalogsMongoDb = mongo.AddAspireMongoDatabase(
    nameOrConnectionStringName: AspireApplicationResources.MongoDatabase.Catalogs,
    databaseName: nameof(AspireApplicationResources.MongoDatabase.Catalogs).ToLowerInvariant()
);
var customersMongoDb = mongo.AddAspireMongoDatabase(
    nameOrConnectionStringName: AspireApplicationResources.MongoDatabase.Customers,
    databaseName: nameof(AspireApplicationResources.MongoDatabase.Customers).ToLowerInvariant()
);

var jaeger = builder.AddAspireJaeger(AspireResources.Jaeger);

var zipkin = builder.AddAspireZipkin(
    AspireResources.Zipkin,
    // because we want to expose the host ui port without a proxy
    proxyEnabled: false
);
var prometheus = builder.AddAspirePrometheus(
    AspireResources.Prometheus,
    configBindMountPath: "./../../../deployments/configs/prometheus.yaml"
);
var loki = builder.AddAspireLoki(
    AspireResources.Loki,
    configBindMountPath: "./../../../deployments/configs/loki-config.yaml"
);
var tempo = builder.AddAspireTempo(
    AspireResources.Tempo,
    configBindMountPath: "./../../../deployments/configs/tempo.yaml"
);

var elastic = builder.AddAspireElasticsearch(AspireResources.ElasticSearch);
var kibana = builder.AddAspireKibana(AspireResources.Kibana, elasticsearch: elastic);

var otelCollector = builder.AddAspireOpenTelemetryCollector(
    nameOrConnectionStringName: AspireResources.OpenTelemetryCollector,
    configBindMountPath: "./../../../deployments/configs/otel-collector-config.yaml",
    waitForDependencies: [jaeger, zipkin, prometheus, loki, tempo, elastic]
);

var grafana = builder.AddAspireGrafana(
    AspireResources.Grafana,
    provisioningPath: "./../../../deployments/configs/grafana/provisioning",
    dashboardsPath: "./../../../deployments/configs/grafana/dashboards",
    waitForDependencies: [jaeger, zipkin, prometheus, loki, tempo]
);

var redis = builder.AddAspireRedis(AspireResources.Redis);

var rabbitmq = builder.AddAspireRabbitmq(
    AspireResources.Rabbitmq,
    pluginsPath: "./../../../deployments/configs/rabbitmq-plugins"
);

var eventstore = builder.AddAspireEventStore(AspireResources.EventStore, logPath: "eventstore_logs");

var catalogsApi = builder
    .AddProject<Projects.FoodDelivery_Services_Catalogs_Api>(
        AspireApplicationResources.Api.Catalogs,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithReference(catalogsPostgres)
    .WaitFor(catalogsPostgres)
    .WithReference(catalogsMongoDb)
    .WaitFor(catalogsMongoDb)
    .WithReference(otelCollector)
    .WaitFor(otelCollector)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis)
    .WithFriendlyApiUrls()
    .WithProjectSwaggerUIUrl()
    .WithProjectAsyncAPIUIUrl()
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    )
    .WithEndpoint(
        "http",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

var customerApi = builder
    .AddProject<Projects.FoodDelivery_Services_Customers_Api>(
        AspireApplicationResources.Api.Customers,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithReference(customersPostgres)
    .WaitFor(customersPostgres)
    .WithReference(customersMongoDb)
    .WaitFor(customersMongoDb)
    .WithReference(otelCollector)
    .WaitFor(otelCollector)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis)
    .WithFriendlyApiUrls()
    .WithProjectSwaggerUIUrl()
    .WithProjectAsyncAPIUIUrl()
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    )
    .WithEndpoint(
        "http",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

var identityApi = builder
    .AddProject<Projects.FoodDelivery_Services_Identity_Api>(
        AspireApplicationResources.Api.Identity,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithReference(identityPostgres)
    .WaitFor(identityPostgres)
    .WithReference(otelCollector)
    .WaitFor(otelCollector)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis)
    .WithFriendlyApiUrls()
    .WithProjectSwaggerUIUrl()
    .WithProjectAsyncAPIUIUrl()
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

var ordersApi = builder
    .AddProject<Projects.FoodDelivery_Services_Orders_Api>(
        AspireApplicationResources.Api.Orders,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WithReplicas(builder.ExecutionContext.IsRunMode ? 1 : 2)
    .WithReference(ordersPostgres)
    .WaitFor(ordersPostgres)
    .WithReference(otelCollector)
    .WaitFor(otelCollector)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis)
    .WithFriendlyApiUrls()
    .WithProjectSwaggerUIUrl()
    .WithProjectAsyncAPIUIUrl()
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    )
    .WithEndpoint(
        "http",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

var apiBff = builder
    .AddProject<Projects.FoodDelivery_Api_Bff>(
        AspireApplicationResources.Api.ApiBff,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WaitFor(catalogsApi)
    .WaitFor(customerApi)
    .WaitFor(identityApi)
    .WaitFor(ordersApi);

var spaBff = builder
    .AddProject<Projects.FoodDelivery_Spa_Bff>(
        AspireApplicationResources.Api.SpaBff,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WaitFor(catalogsApi)
    .WaitFor(customerApi)
    .WaitFor(identityApi)
    .WaitFor(ordersApi);

var gateway = builder
    .AddProject<Projects.FoodDelivery_ApiGateway>(
        AspireApplicationResources.Api.Gateway,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WithExternalHttpEndpoints()
    .WaitFor(apiBff)
    .WaitFor(spaBff)
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
    // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#ports-and-proxies
    // .NET Aspire will parse the launchSettings.json file selecting the appropriate launch profile and automatically generate endpoints
    .WithEndpoint(
        "https",
        endpoint =>
        {
            // - Non-container resources like a project cannot be proxied when both TargetPort and Port are specified with the same value, usually we use Port.
            // - When IsProxy is `true`, proxy uses our launch profile ports, and our app uses a random port, and when IsProxy is `false` the port will use for project prot
            endpoint.IsProxied = true;
        }
    );

var blazorUI = builder
    .AddProject<Projects.FoodDelivery_BlazorWebApp>(
        AspireApplicationResources.Ui.Blazor,
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/networking-overview#launch-profiles
        // https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/launch-profiles#control-launch-profile-selection
        ProfileConstants.HttpsProfile
    )
    .WithExternalHttpEndpoints()
    .WithReference(gateway)
    .WaitFor(gateway);

// https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-nodejs
// https://github.com/dotnet/aspire-samples/tree/main/samples/AspireWithJavaScript
var readctUI = builder
    .AddNpmApp(AspireApplicationResources.Ui.React, "../../UIs/Spa/react-food-delivery")
    .WithReference(gateway)
    .WaitFor(gateway)
    .WithEnvironment("BROWSER", "none")
    // set react `VITE_PORT` env from a generated aspire proxy port
    .WithHttpsEndpoint(port: 5173, name: "https", env: "VITE_PORT")
    .WithExternalHttpEndpoints();

var reactUiHttpsEndpoint = readctUI.GetEndpoint("https");
var reactPublishUrl = "https://localhost:5173";

gateway.WithEnvironment(context =>
{
    context.EnvironmentVariables["CorsOptions__AllowedOrigins__0"] = builder.ExecutionContext.IsRunMode
        ? reactUiHttpsEndpoint.Url
        : reactPublishUrl;
});

spaBff.WithEnvironment(context =>
{
    context.EnvironmentVariables["CorsOptions__AllowedOrigins__0"] = builder.ExecutionContext.IsRunMode
        ? reactUiHttpsEndpoint.Url
        : reactPublishUrl;
    context.EnvironmentVariables["Redirects__ReactSpaAddress"] = builder.ExecutionContext.IsRunMode
        ? reactUiHttpsEndpoint.Url
        : reactPublishUrl;
});

identityApi.WithEnvironment(context =>
{
    context.EnvironmentVariables["ReactSpaAddress"] = builder.ExecutionContext.IsRunMode
        ? reactUiHttpsEndpoint.Url
        : reactPublishUrl;
});

// https://github.com/dotnet/aspire-samples/tree/main/samples/HealthChecksUI
// The actual service endpoints are not exposed in non-development environments by default because of security implications, we expose them through health check ui with authentication
builder
    .AddHealthChecksUI("healthchecksui")
    // This will make the HealthChecksUI dashboard available from external networks when deployed.
    // In a production environment, you should consider adding authentication to the ingress layer
    // to restrict access to the dashboard.
    .WithExternalHttpEndpoints()
    .WithReference(catalogsApi)
    .WithReference(identityApi)
    .WithReference(ordersApi)
    .WithReference(customerApi);

if (builder.ExecutionContext.IsRunMode)
{
    // // https://github.com/scalar/scalar/blob/fbef7e1ee82d7c9e84bc42407e309642dcec5552/documentation/integrations/aspire.md
    // // https://github.com/scalar/scalar/tree/fbef7e1ee82d7c9e84bc42407e309642dcec5552/integrations/aspire
    var scalar = builder
        .AddScalarApiReference(options =>
        {
            options
                .WithTheme(ScalarTheme.Default)
                .WithTestRequestButton()
                .WithSidebar()
                .WithDefaultFonts(false)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            // add auth globally on all endpoints
            var oauthOptions = builder.Configuration.BindOptions<OAuthOptions>();
            var scopesDictionary = oauthOptions.OpenApiScopes.ToDictionary(
                scope => scope,
                scope => $"Access to {scope}"
            );

            options
                .AddPreferredSecuritySchemes(OAuthDefaults.DisplayName)
                .AddAuthorizationCodeFlow(
                    "oauth2",
                    flow =>
                        flow.WithPkce(Pkce.Sha256)
                            .WithAuthorizationUrl($"{oauthOptions.Authority}/connect/authorize")
                            .WithTokenUrl($"{oauthOptions.Authority}/connect/token")
                            .WithSelectedScopes(scopesDictionary.Keys.ToArray())
                            .WithClientId("spa-bff-code-flow")
                );
        })
        .WithApiReference(
            catalogsApi,
            options => options.AddDocument("v1", "catalogs-api-v1").AddDocument("v2", "catalogs-api-v2")
        )
        .WithApiReference(
            customerApi,
            options => options.AddDocument("v1", "customers-api-v1").AddDocument("v2", "customers-api-v2")
        )
        .WithApiReference(
            identityApi,
            options => options.AddDocument("v1", "identity-api-v1").AddDocument("v2", "identity-api-v2")
        )
        .WithApiReference(
            ordersApi,
            options => options.AddDocument("v1", "orders-api-v1").AddDocument("v2", "orders-api-v2")
        );
}
else
{
    builder.AddAspireDashboard(AspireResources.AspireDashboard);
}

// https//learn.microsoft.com/en-us/dotnet/aspire/whats-new/dotnet-aspire-9.3#deployment--publish
var dockerCompose = builder.AddDockerComposeEnvironment("aspire-docker-compose");

// var kubernetes = builder.AddKubernetesEnvironment("aspire-kubernetes");

builder.Build().Run();
