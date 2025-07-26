// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/integrations-overview#hosting-integrations
// Hosting integrations configure applications by provisioning resources (like containers or cloud resources) or pointing to existing instances (such as a local SQL server)

using BuildingBlocks.AspireIntegrations.AspireDashBoard;
using BuildingBlocks.AspireIntegrations.ElasticSearch;
using BuildingBlocks.AspireIntegrations.EventStore;
using BuildingBlocks.AspireIntegrations.Grafana;
using BuildingBlocks.AspireIntegrations.Jaeger;
using BuildingBlocks.AspireIntegrations.Kibana;
using BuildingBlocks.AspireIntegrations.Loki;
using BuildingBlocks.AspireIntegrations.OpenTelemetryCollector;
using BuildingBlocks.AspireIntegrations.Prometheus;
using BuildingBlocks.AspireIntegrations.RabbitMQ;
using BuildingBlocks.AspireIntegrations.Redis;
using BuildingBlocks.AspireIntegrations.Tempo;
using BuildingBlocks.AspireIntegrations.Zipkin;
using BuildingBlocks.Core.Constants;
using BuildingBlocks.Persistence.EfCore.Postgres.Extensions;
using BuildingBlocks.Persistence.Mongo.Extensions;
using FoodDelivery.Services.Shared.Constants;

var builder = DistributedApplication.CreateBuilder(args);

var pgUser = builder.AddParameter("pg-user", value: "postgres", publishValueAsDefault: true);
var pgPassword = builder.AddParameter(name: "pg-password", value: new GenerateParameterDefault { MinLength = 3 }, true);

// // // https://learn.microsoft.com/en-us/dotnet/aspire/whats-new/dotnet-aspire-9.3#deployment--publish
// var dockerCompose = builder.AddDockerComposeEnvironment("aspire-docker-compose");

// // var kubernetes = builder.AddKubernetesEnvironment("aspire-kubernetes");

var postgres = builder.AddAspirePostgres(AspireResources.Postgres, userName: pgUser, password: pgPassword);

postgres.AddAspirePostgresDatabase(AspireApplicationResources.Database.Catalogs);

var mongo = builder.AddAspireMongoDB(AspireResources.MongoDb);

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

var otelCollector = builder.AddAspireOpenTelemetryCollector(
    nameOrConnectionStringName: AspireResources.OpenTelemetryCollector,
    configBindMountPath: "./../../../deployments/configs/otel-collector-config.yaml",
    waitForDependencies: [jaeger, zipkin, prometheus, loki, tempo]
);

var grafana = builder.AddAspireGrafana(
    AspireResources.Grafana,
    provisioningPath: "./../../../deployments/configs/grafana/provisioning",
    dashboardsPath: "./../../../deployments/configs/grafana/dashboards",
    waitForDependencies: [jaeger, zipkin, prometheus, loki, tempo]
);

var elastic = builder.AddAspireElasticsearch(AspireResources.ElasticSearch);

var kibana = builder.AddAspireKibana(AspireResources.Kibana, elasticsearch: elastic);

var redis = builder.AddAspireRedis(AspireResources.Redis);

var rabbitmq = builder.AddAspireRabbitmq(
    AspireResources.Rabbitmq,
    pluginsPath: "./../../../deployments/configs/rabbitmq-plugins"
);

var eventstore = builder.AddAspireEventStore(AspireResources.EventStore, logPath: "eventstore_logs");

if (builder.ExecutionContext.IsPublishMode)
{
    builder.AddAspireDashboard(AspireResources.AspireDashboard);
}

// builder.AddProject<Projects.FoodDelivery_Services_Catalogs_Api>(
//     AspireApplicationResources.Api.Catalogs,
//     ProfileConstants.HttpsProfile
// );

builder.Build().Run();
